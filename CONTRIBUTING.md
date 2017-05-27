# Contributing

If you are reading this, you are interested in contributing to Holographic Photo Project.
Thanks a lot! Holographic Photo Project welcomes contributions from the community.


## Process

You may want to start by reading the [developer manual](https://github.com/Microsoft/Holographic-Photo-Project/blob/master/DEVELOPER.md) before you start contributing.

1. Create an issue.
2. Implement the issue and its tests.
3. Merge dev back in your branch.
4. Start a pull request & address comments.
5. Merge.


## Proposal

For simple tasks like fixing typos and small bug fixes, you can skip this step.

If your change is more than a simple fix, please start by opening an issue describing the problem you want to solve and how you plan to approach the problem.
This will let us have a brief discussion about the problem and, hopefully, identify some potential pitfalls before too much time is spent.


## Implementation

1. Fork the repository. Click on the "Fork" button on the top right of the page and follow the flow.
2. If your work needs more time, then consider branching off of master. Otherwise, just code in your fork.
3. Instructions for building the project and running the tests are in the [README](https://github.com/Microsoft/Holographic-Photo-Project/blob/master/README.md).
4. Make small and frequent commits that include tests, which could be a Unity scene showing usage of your feature.
5. Make sure that all the tests continue to pass.
6. Run StyleCop to make sure your code respects the [DEVELOPER](https://github.com/Microsoft/Holographic-Photo-Project/blob/master/DEVELOPER.md).
6. Ensure the code is [WACK compliant](https://developer.microsoft.com/en-us/windows/develop/app-certification-kit). To do this, generate a Visual Studio solution, right click on project; Store -> Create App Packages. Follow the prompts and run WACK tests. Make sure they all succeed.
7. Ensure you update the [README](https://github.com/Microsoft/Holographic-Photo-Project/blob/master/README.md) with additional documentation as needed.


## Commits

The first line is a summary in the imperative, about 50 characters or less, and should not end with a period.
An optional, longer description must be preceded by an empty line and should be wrapped at around 72 characters.
This helps with various outputs from Git or other tools.

You can update message of local commits you haven't pushed yet using `git commit --amend`.


## Merging dev

To make sure your code is up to date, you should merge dev into your branch. We do not use rebase, since it edits the commit history.
If you want to add new tests for functionality that is not yet written, ensure the tests that are added are disabled.

Don't forget to run `git diff --check` to catch those annoying whitespace changes.


## Pull request

Start a GitHub pull request to merge your topic branch into the [main repository's master branch](https://github.com/Microsoft/Holographic-Photo-Project/tree/master).
(If you are a Microsoft employee and are not a member of the [Microsoft organization on GitHub](https://github.com/Microsoft) yet, please link your Microsoft and GitHub accounts on corpnet by visiting [Open Source at Microsoft](https://opensource.microsoft.com/) before you start your pull request. There are some procedures you will need to complete ahead of time.)
If you haven't contributed to a Microsoft project before, you may be asked to sign a [contribution license agreement](https://cla.microsoft.com/).
A comment in the PR will let you know if you do.

The project maintainers will review your changes. We aim to review all changes within three business days.
Address any review comments, force push to your topic branch, and post a comment letting us know that there are new changes to review.


## Merge

If the pull request review goes well, a project maintainer will merge your changes. Thank you for helping improve Holographic Photo Project!
