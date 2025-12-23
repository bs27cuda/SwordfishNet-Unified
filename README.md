CURRENT VERSION

====================================================================================
About Swordfish Designs and the creative process

Thank you for your interest in this piece of software.
I am a novice programmer with no professional background in computer programming. 
All the work herein is a result of my single-handed work at assembling and adapting
code-behind that, yes, was largely generated with Gemini and GitHub Copilot (more of 
the former, less of the latter). The structure and assembly is my making. 
The UI portion of the software is, I owuld say, over 95% of my own actual writing. 
I love the simplicity of XAML and have learned everything applied here by analysing
existing software that I use, as well as by diving into the properties panel of 
Visual Studio. Very little has been supplied by AI in this aspect.

This is one of several little projects that I have embarked on to learn the basics of
computer programming. I know that there are better, more efficient, more straight-
forward and frankly, more eye-pleasing solutions that achieve what my program does,
and more. The reason for making this is simply the bragging rights of saying, I
created something that works and has a clear purpose. 

====================================================================================
About SwordfishNet Unified

This program initially started as a proof of concept that I could make a program
that accesses my OpenMediaVault server from wherever I was. Used alongside your
preferred VPN solution, or used stand-alone on your home network, this program
aims to give users a single portal to access your OMV dashboard, a terminal to 
access the Linux subsystem on the server hosting computer, as well as a file 
explorer to allow you to manipulate files on noth your server as well as the local
machine. 
Initially, the application was segmented into different windows. The initial reason
for windowed design was to learn how to instantiate and control multiple windows
in a given application. However, I considered the end result to be much too messy
and decided to shift the program to a tabbed window, where users may run the
application in a cleaner environment. 
In my profession (a field completely unrelated to computer programming) we use a
few different pieces of software, and being in a "on-the-road" type of profession,
we use touch-screen laptops to accomplish our work. I developed over the last 15
years a love of touch-screen computing, and decided to make this application as 
touch-screen friendly as I could. It's a given though, I know a Linux terminal
is the furthest thing from touch-screen friendly program you could have.

====================================================================================
Current state of modules

The landing page for the configuration of your connections is pretty much buttoned
up. Failsafes are in place to ensure the user suppies necessary information for the
application to run without issue.

The browser part was the simplest - I can take no credit for this part, because it
simply integrates a stripped down web browser that displays your OMV dashboard page.

The terminal is the most painful part of this program. It is light years away from 
what I hope it can be. I have done enough testing to be reasonably convinced that
it will not cause any issues on the device you are connected to. 

The file explorer also needs a lot of work - not all functions are implemented, and 
I will most probably redo most if not all the UI. It as well, however, seems to be
stable enbough to not cause any server-side or local-side file access. BE WARNED:
file deletion is implemented and functional. The local file deletion should send 
files to the recycle bin ; however, don't rely on my word to base your decisions
on deleting. 

====================================================================================
LEGAL DISCLAIMER

Unless stated differently by your local laws or regulations, this legal disclaimer
applies to any and all parts of the items in the present assembly of code. 

This software is not intended for public release with the purpose of achieving 
any particular work. This software is designed by a person who has no recognized
training in any of the technologies exposed in this code. By choosing to procure
for your own use this software implies that you accept that the software may not
work as intended, may cause irreparable damage to any and all of your devices and
data that may be in contact with any part of this software, and release Swordfish
Designs of any and all responsibility in the case of loss or damage of any data or
equipment in contact with the code. 

Should you procure this software with intent to modify, redistribute, or otherwise
use any and all parts of, you do so implictly accepting to take all responsibility 
of any and all loss or damage of data or equipment, whether it be your own or
another party's. 

PLAIN AND SIMPLE: I am not a professional programmer and I made this software to
learn the basics of programming. Do not expect professional grade software with
professional warranties and guarantees from someone who is not a professional. 

Finally, the current software is assembled using NuGet packages available in the
Visual Studio NuGet browser. Credit goes to Renci, Legend of the Bouncty Castle,
ProKn1fe as well as Microsoft for the modules that are incorporated in the
current program. 
