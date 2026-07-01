import os
import re

def process_file(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content
    
    # Fix [style.width.%]="..."
    content = re.sub(r'\[style\.width\.%\]="([^"]+)"', r'[ngStyle]="{\'width\': (\1) + \'%split_me%\'}"', content)
    content = content.replace("%split_me%", "") # A hack to avoid python f-string issues if there were any, but standard raw string is fine. Wait, replacing `%` with `%` is fine.
    
    # Let's do it cleanly
    content = re.sub(r'\[style\.width\.%\]="([^"]+)"', r'[ngStyle]="{\'width\': (\1) + \'%\'}"', content)
    
    # Fix [style]="'animation-delay: ' + (i * 0.05) + 's'"
    content = re.sub(r'\[style\]="\'animation-delay: \' \+ \((.*?)\) \+ \'s\'"', r'[ngStyle]="{\'animation-delay\': (\1) + \'s\'}"', content)
    
    # Fix style="background: rgba(255,255,255,0.03);"
    content = content.replace('style="background: rgba(255,255,255,0.03);"', 'class="bg-white/[0.03]"')
    
    # Fix onmouseover / onmouseout
    content = content.replace(''' class="transition-colors ext-style-62b75090" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'"''', 
                              ''' class="transition-colors ext-style-62b75090 hover:bg-white/[0.03]"''')
                              
    # Fix sign-in ternary background and border
    content = content.replace(
        '''[style]="error.includes('successful') ? 'background: rgba(16,185,129,0.1); border: 1px solid rgba(16,185,129,0.2)' : 'background: rgba(244,63,94,0.1); border: 1px solid rgba(244,63,94,0.2)'"''',
        '''[ngStyle]="error.includes('successful') ? {'background': 'rgba(16,185,129,0.1)', 'border': '1px solid rgba(16,185,129,0.2)'} : {'background': 'rgba(244,63,94,0.1)', 'border': '1px solid rgba(244,63,94,0.2)'}"'''
    )
    
    content = content.replace(
        '''[style]="role === 'Candidate' ? 'background: rgba(59,130,246,0.1); border: 1px solid rgba(59,130,246,0.3); box-shadow: 0 0 15px rgba(59,130,246,0.1)' : 'background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08)'"''',
        '''[ngStyle]="role === 'Candidate' ? {'background': 'rgba(59,130,246,0.1)', 'border': '1px solid rgba(59,130,246,0.3)', 'box-shadow': '0 0 15px rgba(59,130,246,0.1)'} : {'background': 'rgba(255,255,255,0.03)', 'border': '1px solid rgba(255,255,255,0.08)'}"'''
    )
    
    content = content.replace(
        '''[style]="role === 'Recruiter' ? 'background: rgba(139,92,246,0.1); border: 1px solid rgba(139,92,246,0.3); box-shadow: 0 0 15px rgba(139,92,246,0.1)' : 'background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08)'"''',
        '''[ngStyle]="role === 'Recruiter' ? {'background': 'rgba(139,92,246,0.1)', 'border': '1px solid rgba(139,92,246,0.3)', 'box-shadow': '0 0 15px rgba(139,92,246,0.1)'} : {'background': 'rgba(255,255,255,0.03)', 'border': '1px solid rgba(255,255,255,0.08)'}"'''
    )
    
    content = content.replace(
        '''[style]="role === 'HiringManager' ? 'background: rgba(6,182,212,0.1); border: 1px solid rgba(6,182,212,0.3); box-shadow: 0 0 15px rgba(6,182,212,0.1)' : 'background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08)'"''',
        '''[ngStyle]="role === 'HiringManager' ? {'background': 'rgba(6,182,212,0.1)', 'border': '1px solid rgba(6,182,212,0.3)', 'box-shadow': '0 0 15px rgba(6,182,212,0.1)'} : {'background': 'rgba(255,255,255,0.03)', 'border': '1px solid rgba(255,255,255,0.08)'}"'''
    )
    
    content = content.replace(
        '''[style]="role === 'Admin' ? 'background: rgba(245,158,11,0.1); border: 1px solid rgba(245,158,11,0.3); box-shadow: 0 0 15px rgba(245,158,11,0.1)' : 'background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08)'"''',
        '''[ngStyle]="role === 'Admin' ? {'background': 'rgba(245,158,11,0.1)', 'border': '1px solid rgba(245,158,11,0.3)', 'box-shadow': '0 0 15px rgba(245,158,11,0.1)'} : {'background': 'rgba(255,255,255,0.03)', 'border': '1px solid rgba(255,255,255,0.08)'}"'''
    )
    
    # Fix recruiters message ternary
    content = content.replace(
        '''[style]="message.includes('success') ? 'background: rgba(16,185,129,0.1); border: 1px solid rgba(16,185,129,0.2)' : 'background: rgba(244,63,94,0.1); border: 1px solid rgba(244,63,94,0.2)'"''',
        '''[ngStyle]="message.includes('success') ? {'background': 'rgba(16,185,129,0.1)', 'border': '1px solid rgba(16,185,129,0.2)'} : {'background': 'rgba(244,63,94,0.1)', 'border': '1px solid rgba(244,63,94,0.2)'}"'''
    )
    
    # Fix recruiters job selection ternary
    content = content.replace(
        '''[style]="selectedJobId === job.id ? 'background: rgba(59,130,246,0.1); border: 1px solid rgba(59,130,246,0.25); box-shadow: 0 0 20px rgba(59,130,246,0.08)' : 'background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.06)'"''',
        '''[ngStyle]="selectedJobId === job.id ? {'background': 'rgba(59,130,246,0.1)', 'border': '1px solid rgba(59,130,246,0.25)', 'box-shadow': '0 0 20px rgba(59,130,246,0.08)'} : {'background': 'rgba(255,255,255,0.03)', 'border': '1px solid rgba(255,255,255,0.06)'}"'''
    )

    if content != original:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Fixed bindings in {file_path}")

base_dir = "src/app"
for root, dirs, files in os.walk(base_dir):
    for file in files:
        if file.endswith('.html'):
            process_file(os.path.join(root, file))
