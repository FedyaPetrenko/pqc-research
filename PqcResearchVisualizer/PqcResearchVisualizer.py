import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import re
import os

# --- Configuration ---
RESULTS_DIR = './results'
OUTPUT_DIR = '.'
DPI = 300  # Висока роздільна здатність для друку
SNS_THEME = "whitegrid"
FONT_SCALE = 1.1

def parse_benchmark_log(file_path):
    """Parses BenchmarkDotNet output log to extract the summary table."""
    if not os.path.exists(file_path):
        print(f"Warning: File not found: {file_path}")
        return pd.DataFrame()

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    match = re.search(r'\|\s*Method\s*\|.*', content)
    if not match:
        return pd.DataFrame()

    lines = content[match.start():].split('\n')
    table_lines = []
    
    capture = True
    for line in lines:
        stripped = line.strip()
        if not stripped: continue
        
        if stripped.startswith('|'):
            if '---' in stripped: continue 
            table_lines.append(stripped)
        elif capture and len(table_lines) > 0:
            break

    headers = [h.strip() for h in table_lines[0].strip('|').split('|')]
    data = []
    
    for line in table_lines[1:]:
        values = [v.strip() for v in line.strip('|').split('|')]
        if len(values) == len(headers):
            data.append(dict(zip(headers, values)))

    df = pd.DataFrame(data)

    # --- Data Cleaning & Conversion ---
    def parse_value(val):
        if not val or val == '-': return 0.0
        val = val.replace(',', '')
        m = re.match(r'([\d\.]+)\s*([a-zA-Zμ]+)', val)
        if not m: return 0.0
        
        num = float(m.group(1))
        unit = m.group(2)
        
        if unit == 'ns': return num / 1000
        if unit in ['μs', 'us']: return num
        if unit == 'ms': return num * 1000
        if unit == 's': return num * 1_000_000
        
        if unit == 'B': return num
        if unit == 'KB': return num * 1024
        if unit == 'MB': return num * 1024 * 1024
        
        return num

    if 'Mean' in df.columns:
        df['Mean_us'] = df['Mean'].apply(parse_value)
    if 'Allocated' in df.columns:
        df['Allocated_B'] = df['Allocated'].apply(parse_value)

    # Clean Method Names
    df['Method'] = df['Method'].apply(lambda x: x.split('.')[-1].replace("'", ""))
    
    # --- ТУТ ГОЛОВНА ЗМІНА: ПЕРЕКЛАД ОПЕРАЦІЙ ---
    
    # 1. Витягуємо назву алгоритму (все до останнього пробілу)
    df['Алгоритм'] = df['Method'].apply(lambda x: x.rsplit(' ', 1)[0])
    
    # 2. Витягуємо англійську назву операції
    raw_ops = df['Method'].apply(lambda x: x.rsplit(' ', 1)[1])
    
    # 3. Словник перекладу
    op_map = {
        'KeyGen': 'Генерація ключів',
        'Encap': 'Інкапсуляція',
        'Decap': 'Декапсуляція',
        'Sign': 'Підпис',
        'Verify': 'Перевірка'
    }
    
    # 4. Застосовуємо переклад (якщо слова немає в словнику, залишається як є)
    df['Операція'] = raw_ops.map(op_map).fillna(raw_ops)
    
    return df

def parse_artifact_sizes(file_path):
    """Parses the custom artifact text file."""
    if not os.path.exists(file_path):
        print(f"Warning: File not found: {file_path}")
        return pd.DataFrame()

    data = []
    with open(file_path, 'r', encoding='utf-8') as f:
        for line in f:
            if not line.strip() or line.startswith('-'): continue
            
            clean_line = re.sub(r'\[.*?\]\s*', '', line).strip()
            parts = clean_line.split('|')
            
            algo_name = parts[0].strip()
            
            for p in parts[1:]:
                if ':' not in p: continue
                k, v = p.split(':')
                
                size_match = re.search(r'(\d+)', v)
                if size_match:
                    size = int(size_match.group(1))
                    
                    t_name = k.strip()
                    # Переклад типів об'єктів для графіка артефактів
                    if 'Pub' in t_name: t_name = 'Публічний ключ'
                    elif 'Priv' in t_name: t_name = 'Приватний ключ'
                    elif 'Sig' in t_name: t_name = 'Підпис'
                    elif 'Cipher' in t_name: t_name = 'Шифротекст'
                    
                    data.append({
                        'Алгоритм': algo_name,
                        'Тип': t_name,
                        'Size (Bytes)': size
                    })
    return pd.DataFrame(data)

def plot_bar(df, x, y, hue, title, filename, ylabel, log_scale=False, show_mtu=False):
    plt.figure(figsize=(12, 7))
    sns.set_context("notebook", font_scale=FONT_SCALE)
    
    # Кольорові палітри
    palette = "viridis" 
    if "підпис" in title.lower(): palette = "magma"
    if "об'єктів" in title.lower(): palette = "muted"

    ax = sns.barplot(data=df, x=x, y=y, hue=hue, palette=palette, edgecolor=".2")
    
    if log_scale:
        plt.yscale('log')
        plt.grid(True, which="minor", axis='y', linestyle='--', alpha=0.3)
    
    if show_mtu:
        plt.axhline(y=1500, color='red', linestyle='--', linewidth=2, label='MTU Ethernet (1500 байт)')
        plt.legend(title='Алгоритми та межа')

    # Add Value Labels
    for container in ax.containers:
        labels = []
        for val in container.datavalues:
            if val == 0:
                labels.append("")
            elif val >= 10000:
                labels.append(f'{val/1000:.0f}k')
            elif val >= 1000:
                labels.append(f'{val/1000:.1f}k')
            elif val < 1:
                labels.append(f'{val:.2f}')
            else:
                labels.append(f'{val:.0f}')
        ax.bar_label(container, labels=labels, padding=3, fontsize=10)

    plt.title(title, fontsize=16, fontweight='bold', pad=20)
    plt.ylabel(ylabel, fontsize=12)
    plt.xlabel("") # Прибираємо назву осі X
    
    if not show_mtu:
        plt.legend(title='Алгоритм')

    sns.despine()
    plt.tight_layout()
    
    save_path = os.path.join(OUTPUT_DIR, filename)
    plt.savefig(save_path, dpi=DPI)
    print(f"Згенеровано: {save_path}")
    plt.close()

def main():
    # 1. Load Data
    kem_log = os.path.join(RESULTS_DIR, 'PqcResearchApp.Benchmarks.KemBenchmarks.log')
    sig_log = os.path.join(RESULTS_DIR, 'PqcResearchApp.Benchmarks.SignatureBenchmarks.log')
    art_log = os.path.join(RESULTS_DIR, 'artifact_sizes.txt')

    df_kem = parse_benchmark_log(kem_log)
    df_sig = parse_benchmark_log(sig_log)
    df_art = parse_artifact_sizes(art_log)

    if df_kem.empty or df_sig.empty:
        print("Помилка: Не вдалося обробити логи тестів.")
        return

    # 2. Generate KEM Charts (Інкапсуляція ключів)
    plot_bar(df_kem, x='Операція', y='Mean_us', hue='Алгоритм', 
             title='Швидкодія алгоритмів інкапсуляції ключів (логарифмічна шкала)', 
             filename='kem_execution_time.png', 
             ylabel='Час виконання (мкс)', 
             log_scale=True)
    
    plot_bar(df_kem, x='Операція', y='Allocated_B', hue='Алгоритм',
             title='Інкапсуляція ключів: виділення пам\'яті',
             filename='kem_memory.png',
             ylabel='Обсяг виділеної пам\'яті (байт)')

    # 3. Generate Signature Charts (Цифровий підпис)
    plot_bar(df_sig, x='Операція', y='Mean_us', hue='Алгоритм',
             title='Швидкодія алгоритмів цифрового підпису (логарифмічна шкала)',
             filename='sig_execution_time.png',
             ylabel='Час виконання (мкс)',
             log_scale=True)
             
    # Фільтруємо нульові значення для Verify, щоб графік був чистішим
    df_sig_mem = df_sig[df_sig['Allocated_B'] > 10] 
    plot_bar(df_sig_mem, x='Операція', y='Allocated_B', hue='Алгоритм',
             title='Цифровий підпис: виділення пам\'яті',
             filename='sig_memory.png',
             ylabel='Обсяг виділеної пам\'яті (байт)')

    # 4. Generate Artifact Size Chart (Розміри об'єктів)
    if not df_art.empty:
        plot_bar(df_art, x='Тип', y='Size (Bytes)', hue='Алгоритм',
                 title='Розміри криптографічних об\'єктів (логарифмічна шкала)',
                 filename='artifact_sizes.png',
                 ylabel='Розмір (байти)',
                 log_scale=True,
                 show_mtu=True)

if __name__ == "__main__":
    sns.set_theme(style=SNS_THEME)
    main()