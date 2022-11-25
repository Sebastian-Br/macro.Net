import glob
from msilib.schema import File
import os
import re

working_directory = os.getcwd()
print("Working dir: " + working_directory)
if not (os.path.isdir('.git')):
    print("Not a git repository. Exiting.")
    os.system("pause")
    exit()

project_name = working_directory.split("\\")[-1]
print("Project name: " + project_name)
files_including_vstudio_files = glob.glob(working_directory + '/**/*.cs', recursive=True)
files = []
for file_path in files_including_vstudio_files:
    if (not "\\Debug\\" in file_path) and (not "\\Release\\" in file_path): # exclude visual-studio's internal cs files
        files.append(file_path)

#files now only contains .cs files relevant to the project

project_name_re_escaped = re.escape(project_name)
regex = re.compile("\/\/\/.*?file:\/\/\/.*?\/" + project_name_re_escaped) #re doesn't understand named groups

for code_file_path in files:
    code_file = open(code_file_path, "r")
    code_file_content = code_file.read()
    code_file.close()
    comment_slashes = "/// "
    new_path = "file:///" + working_directory.replace("\\", "/")
    new_path = new_path.replace(" ", "%20")
    full_documentation_string = comment_slashes + new_path
    code_file_content_new = regex.sub(full_documentation_string, code_file_content) #<- fails here if regex is corrupted
    if(code_file_content != code_file_content_new):
        print("Updating file:" + code_file_path)
        new_file_name_temp = code_file_path + ".tmp"
        new_file = open(new_file_name_temp, 'w')
        new_file.write(code_file_content_new)
        new_file.close()
        os.remove(code_file_path)
        os.rename(new_file_name_temp, code_file_path)
print("done!")
os.system("pause")