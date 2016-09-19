Echo On

xsd DataStructure.xml
xsd DataStructure.xsd /c /n:"BEGAFamilyUpdater.DataStructure" /classes
del DataStructure.xsd
rem copy /A/Y .\DataStructure.cs .\DataStructure.new
rem pause
rem Powershell.exe -executionpolicy remotesigned -File  C:\Daten-Arbeit\Extern\BEGA\src\BEGAFamilyUpdater\BEGAFamilyUpdater\Data\Replacement.ps1
