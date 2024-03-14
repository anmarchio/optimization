import xml.etree.ElementTree as ET
import os
import os.path as path
"""
This primitive script traverse the checks each folder for a csproj file, loads it, scans for the halcon reference and replaces it by the halcon root
this should work like a charm if you have only one version of halcon installed. Else the halcon root might be ambiguous or set to the wrong halcon installation.

was only tested with halcon 13 halconroot, but should work with 12 as well.

This script should spare you the trouble to edit each reference in the solution manually. 

ATTENTION: script is supposed to be run from parent directory (else the relative references below will not work), i.e.

python Scripts/fix_halcon_reference.py
"""
wd = os.getcwd()
prime = os.listdir(wd)
ns = "http://schemas.microsoft.com/developer/msbuild/2003"
halcon_root = "$(HALCONROOT)"#os.environ['HALCONROOT']

#print('halcon_root: {}'.format(halcon_root))

for dir in prime:
	if path.isdir(dir):
		# enter dir, check for csproj
		proj_dir = path.join(wd, dir)
		files = [x for x in os.listdir(proj_dir) if x.endswith('.csproj')]
		for proj in files:
			file_name = path.join(proj_dir, proj)
			ET.register_namespace('', ns)
			tree = ET.parse(file_name)
			root = tree.getroot()
			groups = root.findall('{' + ns + '}ItemGroup')
			for grp in groups:
				references = grp.findall('{' + ns + '}Reference')
				if len(references) > 0:
					for ref in references:
						attr = ref.attrib
						if 'halcondotnet' in attr['Include']:
							hint_path = ref.find('{' + ns + '}HintPath')
							p = path.join(halcon_root, hint_path.text[hint_path.text.find('bin'):])
							print('replacing: {} \n with: {}'.format(hint_path.text, p))
							hint_path.text = p
			tree.write(file_name, xml_declaration=True,
					   encoding='utf-8', method='xml')
			print("written to: {}".format(file_name))
			pass