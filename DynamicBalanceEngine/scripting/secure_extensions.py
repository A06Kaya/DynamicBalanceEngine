ALLOWED_EXTENSIONS = {'pdf', 'png', 'jpg'}

def validate_filename(filename):
    """
    Validates if the filename has an allowed extension.
    
    Args:
        filename (str): The name of the file.
        
    Returns:
        bool: True if valid, False otherwise.
    """
    if '.' not in filename:
        return False
        
    ext = filename.rsplit('.', 1)[1].lower()
    return ext in ALLOWED_EXTENSIONS

if __name__ == "__main__":
    # Test cases
    files = ["document.pdf", "image.png", "script.exe", "photo.jpg", "malware.sh"]
    
    for f in files:
        is_valid = validate_filename(f)
        print(f"File: {f}, Valid: {is_valid}")
