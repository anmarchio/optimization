# Wiki

* [Commandline](commandline.md)
* [Programming-Guidelines](programming-guidelines.md)
* [REST communication between Optimization and Webapp](rest.md)
* [Testing-Guidelines](testing.md)
* [Troubleshooting](troubleshooting.md)
* [gitlab runner](gitlab-runner.md)
* [Home](home.md)

## Programming-Guidelines

This page briefly describes some practices that we would like everyone to adhere to. They are very rough guidelines and might be added to or changed in the future.

### Coding Conventions

We should all aspire to adhering to the code conventions outlined here: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions

Most public methods should be commented. If you find that you use a method that you have difficulty understanding and need to think about the implementation, please enhance its current documentation in a way that you feel would have helped you understand it. Not everything needs to be commented, though. Try to name things good enough for simple comments to be redundant.

Much legacy code you will see does not necessarily follow above guidelines. However, in a continued effort to improve the quality of the code, we should not follow poor examples from the past.

Further all programmers new to c# should make themselves aquainted with this further c#-conventions concerning structuring classes and creating correct hierarchies.

The following repository shows the expected order of methods, properties, functions, constructors:
https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1201.md

Thsi corresponding thread illustrates the suggested conventions for correct code:
https://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods.

On this node a reminder for UpperCamelCase for properties:D

By chance during the natrual workflow please adapt classes not following the guidelines:)

### Workflow

For most feature implementation tasks you should start with the development branch as your base. The master branch is often outdated and only represents the last "working" state of the application.

Most things you do should be tracked by an issue here in this repository. Chances are that you were assigned one or more issues. If you have any questions regarding the instructions or any doubt about the purpose of the task specified in some issue, do not hesitate to discuss them in a comment to that issue. 

If you encounter a bug while working on an issue, feel free to create a separate issue that describes the bug. You do not necessarily have to fix it yourself, but if you do, please describe the steps you took. This should make it easier to anyone merging  your fix to understand what you did.

Most enhancement issues should be worked on using an individual feature branch. Once you feel like you're done, push your results, check if all tests pass and remove the WIP status of your merge request.

If you find the documentation of some part of the code to be lackluster, create a new issue detailing what you would like to have explained better.

For some types of issues there are templates, which can be selected via drop down when creating a new issue. Use as appropriate; they should at least be useful as a suggestion of what the issue should include. We'll try to expand the list of templates as much as possible.

#### Step by Step

Here a quick step-by-step example of a simple workflow. Feel free to deviate from it, if it makes more sense to you. I'm assuming that you have already cloned the repository. Please let leen or mara know if you start working on a new issue, so that we can schedule a pair-programming session to introduce you to the relevant portion of the code.

0. Move your issue on the issue board to "Doing"
1. Make sure that you are branching from development: ```git checkout development``` (if necessary, update your development branch: ```git fetch```, ```git pull```)
2. Create a new feature branch: ```git checkout -b feature/<descriptive name>```
3. Do some work. The sooner you push something to the remote repository, the sooner the feedback loop can kick in. Please take care not to commit binary files. Only source files should reside in the repository. You can do so by individually committing files. If you are unfamiliar with git, please look at one of the many tutorials on the internet or look at a [cheat sheet](https://www.atlassian.com/git/tutorials/atlassian-git-cheatsheet). You can also add directories and files to .gitignore if you want to ensure that they are not committed by accident.
4. Push your changes and create a merge request with WIP (Work in Progress) flag. Just follow the Gitlab instructions.

#### Accetability Criteria for Merge Requests

Typically, all issues contain a few acceptability critera for merge requests. Additionally, you should also ensure that:

1. Most public methods and constructors are documented in such a way, that people can use them without having to read the implementation. Use Visual Studios comment template for this. (e.g. by writing /// above a method and let autocomplete do the rest for you or right click -> Snippet -> Insert Comment)

2. Wherever possible, we recommend following the test-driven development workflow. However, as much of what we do is somewhat experimental, this might not always be feasible. There should be a few basic tests (unit tests, where they make sense) and at the very least functional tests that use the code you wrote and demonstrate that it does not break immediately on a different machine.

3. Code Review. At least one other person has to review the changes and approve of them.

4. Document challenges, decisions, etc. in the issue that you are working on. The issue (even once closed) can be linked to individual commits and thus to merge requests. Just add #<issue number> to your commit message and gitlab will link the two. You can also reference merge requests directly with !<merge request number>.

Basically in order to complete the merge request the code has to be: commented, documented, tested and reviewed.

You will notice that gitlab will run automatic test and build pipelines whenever you push something. Those tests must pass for a successful merge request.

### Structure of the Solution

C# in Visual Studio are structured in projects (.csproj) and solutions (.sln), with the latter being collections of the former. I.e. any solution is structured in one or more projects. Any project can be part of one or more solutions.

**A typical starting point for you should be Optimization.sln, which includes all projects.** Other solutions are there to exclude projects that reference libraries that are not available under mono (such as RegionMarker.csproj)

This application is currently aimed at supporting two backends for optimizing image processing pipelines:

1. Halcon
2. OpenCv

The structure of the individual projects reflects this. Anything that is OpenCv specific resides in Optimization.CVPipeline.csproj (and Optimization.CVPipeline.Tests.csproj) and similarly for Halcon. **The idea is that the OpenCv part of the program should be compilable  and executable without a halcon license, and vice versa.**

**Anything that does not directly involve any reference to Halcon or OpenCv (or any library supported in the future) should reside in Optimization.csproj**

Utility methods that do not require special dependencies should reside in Extensions.csproj

Special projects are:
* Optimization.Commandline.csproj, which contains all code for executing the program as a commandline application
* Optimization.StartUp.csproj, which is used to communicate with the django frontend via the database (to be deprecated)
* RegionMarker.csproj, which contains a labeling tool that produces labeled data compatible with the both the halcon and opencv backend (the latter by converting the former)
* Extensions.csproj, which contains utility functions (typically in form of extension methods), that are agnostic of any supported backend
* a number of .Tests projects, that contain project specific tests.

### Using external libraries

Feel free to use any library with a compatible license. However **make sure that it is also mono compatible**, or else the program won't run under linux.