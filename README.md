# Monogame-CJK-TTF
Dynamic rendering of TTF fonts, with a focus on Chinese characters.


This is more of a last resort, you're better off using a Monogame spritefont, but if you need rare characters, say for multiplayer chat or something this could fit.
Was written fast as a quick solution, so it's rough in some areas.

A custom FontDescription class to load common characters is a faster solution if you can get away with only common characters.
Monogame 3.7 has or will have that feature by default.
Here's a resource for that(Chinese): https://www.xnadevelop.com/ios/use-chinese-in-monogame/

##### In the case you need rare characters, you can use this, but GOOD LUCK!

See the [SAMPLE PROJECT](/Sample/Game.Desktop/Game.cs) for how to use this project.

Drawing characters for the first time is VERY slow due to the dynamic nature of it, try to draw common characters early so it'll cache them. I'll make it cache to a file at some point so that it doesn't have to cache again every time it's launched.
