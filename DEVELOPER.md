# Developer documentation

If you are reading this, you are interested in contributing to Holographic Photo Project.
Thanks a lot!  Holographic Photo Project welcomes contributions from the community.


## Coding conventions

We believe that following consistent coding conventions results in more readable code, and improves the overall quality of the software. Here is a list of the coding conventions you should follow while contributing to this project.


### Naming

- **Properties** : `PropertiesBeginWithUpperCaseLetter`
- **Fields** : `fieldsBeginWithLowerCaseLetter` (do not prefix with `_`)
- **Public fields** : `PublicFieldBeginWithUpperCaseLetter`
- **Interfaces** : `IInterfacesBeginWithLetterI`
- **Constants** : `ConstantsBeginWithUpperCaseLetter`
- **Classes** : `ClassesBeginWithUpperCaseLetter`


### Layout

- Use Allman style braces
- Never omit braces
- Put a blank line after a closing bracket
- Avoid using more than one blank line
- Use 4 spaces indentation instead of tabs


### Maintainability

- Always declare access modifier
- Put only one class per file (excluding nested classes)
- Prefer pre-increments when post-increment is not necessary
- Put constants in a private static nested class named Constants
- Never use `#regions`
- Use `var` when the type is obvious


### Documentation

Comments are useful, but hard to maintain. We think sometimes, less is better!

- `// Start your comments with a space and a capital letter.`
- Put comments over cryptic math expressions, algorithms and unclear code
- Put summaries over public methods and properties, and even private ones if it makes sense


## Creating a new feature

If you plan on creating a new feature, please start an [issue]([TODO : link to the issue page]) beforehand and explain how you plan on approaching the problem. This will let us have a brief discussion about the problem and, hopefully, identify some potential pitfalls before too much time is spent.


### Test scene (applies only to big features)

To prove your feature is working, and so we can know it is still working in the future, we ask you to create a test scene. This scene should be the simplest possible and should showcase how your component works.
