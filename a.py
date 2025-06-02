import os
import argparse

def find_oldest_file(directory):
    oldest_file = None
    oldest_time = float('inf')
    
    for root, _, files in os.walk(directory):
        for file in files:
            file_path = os.path.join(root, file)
            try:
                file_time = os.path.getctime(file_path)  # Get file creation time
                if file_time < oldest_time:
                    oldest_time = file_time
                    oldest_file = file_path
            except Exception as e:
                print(f"Error accessing {file_path}: {e}")
    
    return oldest_file

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Find the oldest file in a directory recursively.")
    parser.add_argument("directory", type=str, help="Path to the directory")
    args = parser.parse_args()
    
    oldest = find_oldest_file(args.directory)
    
    if oldest:
        print(f"Oldest file: {oldest}")
    else:
        print("No files found in the directory.")
