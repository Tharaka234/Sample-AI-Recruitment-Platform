import os
import re
import hashlib

def get_class_name(style_content):
    # Create a unique class name based on the style content
    hash_obj = hashlib.md5(style_content.encode())
    return "ext-style-" + hash_obj.hexdigest()[:8]

def process_file(file_path, extracted_styles):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # regex to find <tag ... style="...">
    tag_pattern = re.compile(r'(<\w+[^>]*?)(\s+style="([^"]*)")([^>]*>)', re.DOTALL)
    
    def tag_replacer(match):
        pre_style = match.group(1)
        style_attr = match.group(2)
        style_content = match.group(3).strip()
        post_style = match.group(4)
        
        if not style_content:
            return match.group(1) + match.group(4)
            
        class_name = get_class_name(style_content)
        extracted_styles[class_name] = style_content
        
        # Check if there is already a class attribute in pre_style or post_style
        class_pattern = re.compile(r'class="([^"]*)"')
        
        if class_pattern.search(pre_style):
            pre_style = class_pattern.sub(r'class="\1 ' + class_name + '"', pre_style)
            return pre_style + post_style
        elif class_pattern.search(post_style):
            post_style = class_pattern.sub(r'class="\1 ' + class_name + '"', post_style)
            return pre_style + post_style
        else:
            # No class attribute exists, add one
            return pre_style + f' class="{class_name}"' + post_style

    new_content = tag_pattern.sub(tag_replacer, content)
    
    if new_content != content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {file_path}")

def main():
    extracted_styles = {}
    base_dir = "src/app"
    
    for root, dirs, files in os.walk(base_dir):
        for file in files:
            if file.endswith('.html'):
                file_path = os.path.join(root, file)
                process_file(file_path, extracted_styles)
                
    if extracted_styles:
        styles_path = "src/styles.css"
        with open(styles_path, 'a', encoding='utf-8') as f:
            f.write("\n/* Extracted Inline Styles */\n")
            for class_name, style_content in extracted_styles.items():
                f.write(f".{class_name} {{\n  {style_content}\n}}\n")
        print(f"Added {len(extracted_styles)} new classes to {styles_path}")

if __name__ == "__main__":
    main()
