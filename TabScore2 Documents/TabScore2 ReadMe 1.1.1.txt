TabScore2 ReadMe file

TabScore2 is free and open source software that provides wireless scoring for 
the card game bridge.  It is a Windows desktop application that serves active
web pages across a local wireless network.  It requires a server PC, a local 
wireless network, and some sort of table-top device with a web browser on each 
table (tablet, phone, Kindle, etc).

TabScore 2 uses the Bridgemate .bws standard Access database that is created by
the scoring program, so is a direct replacement for BridgeTab (or Bridgemate, 
BridgePad, etc).  Although TabScore2 has been built with EBUScore in mind, it 
should work with any bridge scoring program that can create a .bws database.

TabScore2 is designed for use on a PC with Windows 10+ and .NET 8.  It is now a 
64-bit applciation, although is still requires a 32-bit process, GrpcBwsDatabaseServer, 
to read and write to the .bws database.

There are 3 .NET 8 runtime libraries (ASP.NET Core Runtime 8, .NET Desktop Runtime 8 
and .NET Runtime 8), and TabScore 2 requires both the 32-bit (x86) and 64-bit (x64)
versions (6 in total).  To check if these are installed, look in the appropriate 
folders in "C:\Program Files(x86)\dotnet\shared\" and "C:\Program Files\dotnet\shared\".
If not installed, they can be downloaded from Microsoft.

Because TabScore2 uses Bootstrap 5, the browser and tablet operating systems must 
meet certain minimum standards.  For Android, this means version 6.0+.  Please check 
here for more details: getbootstrap.com/docs/5.0/getting-started/browsers-devices.

To install TabScore2, simply copy the entire TabScore2 folder to "C:\Program Files".

TabScore2 is currently limited to 4 sections (A, B, C and D in that order) and 
30 tables per section.  It can be used for pairs, teams, Swiss events, and  
individual events (provided the scoring program supports them).

TabScore2 can operate in 3 modes: Traditional Mode with one table-top device 
per table (like Bridgemate); Personal Mode where the table-top devices
move with players; and Scorer Mode which is similar to Personal Mode but allows
the scorer at the table to be chosen each round.  Personal Mode and Scorer Mode
allow players to use their own tablets or phones, avoiding the need for bridge 
clubs to invest in expensive hardware.

TabScore2 implements a range of display options which can be set by the
scoring program, or from its Settings window.  See the User Guide for more
details.

TabScore2 is available in English, Spanish, German and Dutch.  It will default 
to English if the Windows language is not supported.

TabScore2 uses Bo Haglund's Double Dummy Solver (DDS) to analyse 
hand records.  DDS requires the Microsoft Visual C++ Redistributable (x64) 
2015 (or later) to be installed on the PC.

See the NOTICE and LICENSE files.