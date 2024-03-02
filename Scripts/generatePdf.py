import pdfkit
import sys
import os
from pyvirtualdisplay import Display

try:
   display = Display(visible=0, size=(600,600))
   display.start()
   script_dir = os.path.dirname(os.path.abspath(__file__))
   target_file = os.path.join(script_dir, '..', sys.argv[2])
   pdfkit.from_string(sys.argv[1], target_file)
finally:
   display.stop()

print(f"OK: {target_file}")