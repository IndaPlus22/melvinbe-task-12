﻿# 🐜 Antoids 🐜
A simulation of "flocking" behavior in ants!

## Cool Algorithms

### 🐜 Ant Behavior
Ants are autonomous agents that move around randomly when on their own. However, together they can form trails transporting food to their nest. This complex behaviour is the result of two very simple rules:
* Ants looking for food leave red pheromones and follow green pheromones.
* Ants that are carrying food leave green pheromones and follow red pheromones.

### 🌫️ Perlin Noise
Perlin noise is used to generate a random but smooth 2D grid of numbers to represent caves.

### 🪨 Marching Squares
The marching squares algorithm is used to convert the grid generated by the perlin noise into a smooth shape that can be rendered. It works by considering four points in the grid at a time, and looking up a shape from a list to best represent those points. Linear interpolation is also used to apply further smoothing to the result. 

## Instructions

### Build
* Open the project solution in Visual Studio 2022 (2019 or lower may or may not work).
* You probably need to have <b>MonoGame.Framework.DesktopGL</b> and <b>MonoGame.Content.Builder.Task</b> installed as NuGet packages.
* Build the application through Visual studio.
 
### Run
* Run the .exe file located at <b>\bin\Release\net6.0\Antoids.exe</b>.

## Known issues
* Edges of the window are not covered in dirt.
* Placing and removing terrain is horrible performance-wise. Needs optimisation.
* The ants being generally stupid.

## Cool and Inspirational Videos
* Smarter ants solve mazes by [Pezzza](https://youtu.be/V1GeNm2D2DU).
* A similar simulation in Unity by [Sebastian Lague](https://youtu.be/X-iSQQgOd1A).
* A very good explanation of the Marching Squares algorithm by [Daniel Shiffman himself](https://youtu.be/0ZONMNUKTfU).

## 🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜🐜
