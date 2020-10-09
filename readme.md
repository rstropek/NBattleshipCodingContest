![Build](https://github.com/rstropek/NBattleshipCodingContest/workflows/.NET%20Core/badge.svg)

# Battleship Coding Contest

## Introduction

The purpose of this project is to teach people how to program in C# and .NET. It does this in two ways:

1. Beginners can learn the basics of C# by programming computer players for the classical [*Battleship* game](https://en.wikipedia.org/wiki/Battleship_(game)). In a class, each student can write her own player and afterwards they can check whose player is the best.

2. Advanced programmers can take a look at the implementation of the underlying runtime which is also written in C# and .NET. Trainers can use the sample to demonstrate various C# and .NET features working in a larger, non-trivial code base.

## History

The history of this projects goes back many years. At that time, a group of developers at the company *cubido* wanted to demonstrate the capabilities of *ASP.NET* and *Silverlight* to customers. I (Rainer Stropek) was one of them. We implemented a coding challenge that we called *C# Pirates*. It allowed people to write and upload computer players for *Battleship*. In a rich GUI, players could play against each other and a leaderboard was continuously filled. The overall winners were awarded with gifts.

This project is a complete rewrite and works differently from the original *C# Pirates*. However, the basic idea of allowing people to write computer players for *Battleship* in C# has remained the same. Here is a list of workshops and courses in which it was used:

* The first version of this project was created for a workshop at the German *BASTA* conference in 2020 ([presentation](https://slides.com/rainerstropek/battleship-workshop/fullscreen), [YouTube recording of workshop (German)](https://www.youtube.com/playlist?list=PLhGL9p3BWHwvEzTmbe5cUwR4oqQr9dl-8))
* C# mini coding challenge in the [CoderDojo Linz](https://linz.coderdojo.net/) (Oct. 2020)

Did you use this project in one of your workshops or courses? Please add it to the list above by sending a pull request.

## Note Source Generator

To build the project, you currently need to add `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json` as a NuGet package source. The reason for this is that the project uses Roslyn's new *C# Source Generators*. You need the package source for its current preview packages.

## Documentation

See [project wiki](https://github.com/rstropek/NBattleshipCodingContest/wiki).
