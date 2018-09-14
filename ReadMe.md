# Vulpine Core Library: Artificial Intelligence

http://www.jakesden.com/corelibrary.html

The Vulpine Core Library is a collection of software libraries (assemblies) written in the C# programing language. It is my personal repository of code, in that I am the sole developer. However, I have opened it up to the public, under the GNU Lesser General Public License (LGPL) version 2.1; more on that below. It serves as the backbone of most of my homegrown video games and applications. The fractals and other computer generated art I have on display were created using the code from this library. 

The library itself is continuously evolving, as I am constantly adding to it, updating it whenever I find something that tickles my fancy. I have big plans for stuff that I want to include in the library, and it is far from completion. I doubt it will ever be complete. Due to the dynamic nature of the library, I cannot guarantee backwards compatibility, so expect to have to re-factor your code if you use are updating from a previous version.

This sub-library involves artificial intelligence, neural networks, machine learning, and genetic algorithms. It contains various algorithms to train an evolve different types of neural networks, for use in classification problems, strategic game-play, and computer generated art, among others. It also features more classical approaches to artificial intelligence. It is meant to be a comprehensive, though not exhaustive, collection of problem solving methods.

All documentation for the library is included in the code itself. Every public method and class is heavily commented, providing intellisense for things like parameter use and expected output. I hope it is clear-cut what each method dose, and its intended use, from the documentation I have provided. If not, at least I tried my best. I hope that you will find my code useful, and intuitive, and perhaps maybe even learn something from it.

# Explanation of the LGPL

The GNU Lesser General Public License is a complex legal document that confuses many people. Now I am neither a lawyer nor a member of the FSF, so I should state that what follows is purely my own interpretation, and should not be used in a legal context. Instead the primary purposes of this document is to alleviate some of the confusion regarding the LGPL as it relates to the Vulpine Core Library. Hopefully, that way, people can more confidently use my library as I, the author, have intended.

Basically the LGPL allows you to use the software package as a library, without placing any restrictions on the programs that use it. That is, you may freely link with and redistribute the unaltered library as long as you both A) make it known to your end users that you are using this library and that this license applies to it, and B) allow your end users to obtain the source code for the unaltered library, if they so desire.

Your code, that is the code that links with the library, need not be open source, as the rules governing the LGPL do not apply to it. However, if you modify the library itself, so as to compile a new version of the library, then those modifications must be released under the LGPL or a compatible license. In this case, you are only required to open-source changes to the library itself, and not any other code. Of course, if your project is open-source anyway, then you need not worry about this requirement.

The full legal text of the LGPL goes into greater detail about these specific use cases, and what exactly is required of the licensor in each case. In terms of the Vulpine Core Library, as long as you are linking against the DLL files, that is the files you obtain when compiling the library, you should (in theory) have no problem meeting the terms of the LGPL. To be absolutely certain though, you should check the full legal text of the LGPL. 

The main reason why I decided to release the Vulpine Core Library under the LGPL, instead of a more restrictive license like the GPL, was so that it could be used in conjunction with other software libraries without creating a license conflict. If I had released the library under the GPL, then it could only ever be used with GPL compatible libraries. By releasing it under the LGPL, my library can (in principle) be used in conjunction with ANY other library, regardless of license requirements.

That said, there may be certain libraries which are incompatible with the terms of the LGPL. It is my hope, that these cases are exceptionally rare. However if you find yourself needing to use the Vulpine Core Library in conjunction with such a library, please contact me. As it is my intent that the Vulpine Core Library should be usable in any combination, I may be able to grant you special permission to use the library under such circumstances.
