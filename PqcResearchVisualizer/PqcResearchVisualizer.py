import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import re
import os

# --- Configuration ---
RESULTS_DIR = './results'
OUTPUT_DIR = '.'
DPI = 300  # High resolution for publication
SNS_THEME = "whitegrid"
FONT_SCALE = 1.1

def parse_benchmark_log(file_path):
    """Parses BenchmarkDotNet output log to extract the summary table."""
    if not os.path.exists(file_path):
        print(f"Warning: File not found: {file_path}")
        return pd.DataFrame()

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find the table header
    match = re.search(r'\|\s*Method\s*\|.*', content)
    if not match:
        return pd.DataFrame()

    # Extract lines starting from header
    lines = content[match.start():].split('\n')
    table_lines = []
    
    # Simple state machine to capture table rows
    capture = True
    for line in lines:
        stripped = line.strip()
        if not stripped: continue
        
        if stripped.startswith('|'):
            # Stop if we hit a separator line after data (usually not needed if we check format)
            if '---' in stripped: continue 
            table_lines.append(stripped)
        elif capture and len(table_lines) > 0:
            # Stop capturing when we hit a non-table line
            break

    # Parse CSV-like structure
    headers = [h.strip() for h in table_lines[0].strip('|').split('|')]
    data = []
    
    for line in table_lines[1:]:
        values = [v.strip() for v in line.strip('|').split('|')]
        if len(values) == len(headers):
            data.append(dict(zip(headers, values)))

    df = pd.DataFrame(data)

    # --- Data Cleaning & Conversion ---
    
    def parse_value(val):
        """Converts strings like '524,049.09 μs' to float (microseconds)"""
        if not val or val == '-': return 0.0
        val = val.replace(',', '')
        
        # Regex to find number and unit
        m = re.match(r'([\d\.]+)\s*([a-zA-Zμ]+)', val)
        if not m: return 0.0
        
        num = float(m.group(1))
        unit = m.group(2)
        
        # Normalize time to microseconds (μs)
        if unit == 'ns': return num / 1000
        if unit in ['μs', 'us']: return num
        if unit == 'ms': return num * 1000
        if unit == 's': return num * 1_000_000
        
        # Normalize memory to Bytes
        if unit == 'B': return num
        if unit == 'KB': return num * 1024
        if unit == 'MB': return num * 1024 * 1024
        
        return num

    # Convert numeric columns
    if 'Mean' in df.columns:
        df['Mean_us'] = df['Mean'].apply(parse_value)
    if 'Allocated' in df.columns:
        df['Allocated_B'] = df['Allocated'].apply(parse_value)

    # Clean Method Names: "SignatureBenchmarks.RSA-4096 KeyGen" -> "RSA-4096 KeyGen"
    df['Method'] = df['Method'].apply(lambda x: x.split('.')[-1].replace("'", ""))
    
    # Split into Algorithm and Operation
    # Assumes format "AlgoName Operation" (e.g., "RSA-4096 KeyGen")
    df['Algorithm'] = df['Method'].apply(lambda x: x.rsplit(' ', 1)[0])
    df['Operation'] = df['Method'].apply(lambda x: x.rsplit(' ', 1)[1])
    
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
            
            # Remove [Tags]
            clean_line = re.sub(r'\[.*?\]\s*', '', line).strip()
            parts = clean_line.split('|')
            
            algo_name = parts[0].strip()
            
            for p in parts[1:]:
                if ':' not in p: continue
                k, v = p.split(':')
                
                # Parse "1206 B"
                size_match = re.search(r'(\d+)', v)
                if size_match:
                    size = int(size_match.group(1))
                    
                    # Normalize type names
                    t_name = k.strip()
                    if 'Pub' in t_name: t_name = 'Public Key'
                    elif 'Priv' in t_name: t_name = 'Private Key'
                    elif 'Sig' in t_name: t_name = 'Signature'
                    elif 'Cipher' in t_name: t_name = 'Ciphertext'
                    
                    data.append({
                        'Algorithm': algo_name,
                        'Type': t_name,
                        'Size (Bytes)': size
                    })
    return pd.DataFrame(data)

def plot_bar(df, x, y, hue, title, filename, ylabel, log_scale=False, show_mtu=False):
    plt.figure(figsize=(12, 7))
    sns.set_context("notebook", font_scale=FONT_SCALE)
    
    # Determine Palette
    palette = "viridis" if "KEM" in title else "magma"
    if "Artifact" in title: palette = "muted"

    ax = sns.barplot(data=df, x=x, y=y, hue=hue, palette=palette, edgecolor=".2")
    
    if log_scale:
        plt.yscale('log')
        plt.grid(True, which="minor", axis='y', linestyle='--', alpha=0.3)
    
    if show_mtu:
        plt.axhline(y=1500, color='red', linestyle='--', linewidth=2, label='Ethernet MTU (1500 B)')
        plt.legend()

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
    plt.xlabel("")
    sns.despine()
    plt.tight_layout()
    
    save_path = os.path.join(OUTPUT_DIR, filename)
    plt.savefig(save_path, dpi=DPI)
    print(f"Generated: {save_path}")
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
        print("Error: Could not parse benchmark logs. Check file paths.")
        return

    # 2. Generate KEM Charts
    # Execution Time (Log Scale due to RSA difference)
    plot_bar(df_kem, x='Operation', y='Mean_us', hue='Algorithm', 
             title='KEM Performance: Execution Time (Log Scale)', 
             filename='kem_execution_time.png', 
             ylabel='Time (Microseconds) - Log Scale', 
             log_scale=True)
    
    # Memory Allocation
    plot_bar(df_kem, x='Operation', y='Allocated_B', hue='Algorithm',
             title='KEM Memory Allocation',
             filename='kem_memory.png',
             ylabel='Allocated Memory (Bytes)')

    # 3. Generate Signature Charts
    # Execution Time (Log Scale)
    plot_bar(df_sig, x='Operation', y='Mean_us', hue='Algorithm',
             title='Digital Signature Performance (Log Scale)',
             filename='sig_execution_time.png',
             ylabel='Time (Microseconds) - Log Scale',
             log_scale=True)
             
    # Memory Allocation (Filter out zero-allocation Verify methods for cleaner chart)
    df_sig_mem = df_sig[df_sig['Allocated_B'] > 10] 
    plot_bar(df_sig_mem, x='Operation', y='Allocated_B', hue='Algorithm',
             title='Digital Signature Memory Allocation',
             filename='sig_memory.png',
             ylabel='Allocated Memory (Bytes)')

    # 4. Generate Artifact Size Chart
    if not df_art.empty:
        plot_bar(df_art, x='Type', y='Size (Bytes)', hue='Algorithm',
                 title='Cryptographic Artifact Sizes (Log Scale)',
                 filename='artifact_sizes.png',
                 ylabel='Size (Bytes) - Log Scale',
                 log_scale=True,
                 show_mtu=True)

if __name__ == "__main__":
    sns.set_theme(style=SNS_THEME)
    main()