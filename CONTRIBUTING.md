# Contributing

So, you're thinking about contributing to Itinero. Awesome! We have a few basic rules to make sure maintaining Itinero is sustainable with limited time available.

### What can you do to help?

- Documentation & samples: If you need or find undocumented features. Or you asked a question please think about documenting this for future users. You can also contribute a sample application if the use case is generic enough. Otherwise, just upload the sample project to a seperate repo and we can link to it.
- Bug reports: Very important, if something doesn't work as expected please [report the issue](https://github.com/itinero/routing/issues). We built Itinero for some use cases but perhaps you have a new one or you stumbled onto a bug. We need to know!
- Bug fixes: Even better, if you can fix something, please do!
- Feature requests: Need something that's not there, add it to the [issues](https://github.com/itinero/routing/issues), maybe someone already implemented what you need. Also check our [roadmap](https://github.com/itinero/routing/wiki/Roadmap) before doing this.
- Spread the word: A project like this grows and improves when usage increases and we have more eyes on the code. So spread the word by writing blog posts or tweet about this awesome library!

### How to contribute?

First of all don't be afraid to contribute. Most contributions are small but very valuable. Some basic rules to make things easier for everyone:

- Don't change more than you need to to fix something or implement a new feature. No 'resharping' or refactoring please.
- Think about more than yourself, this may seem strange, but Itinero is used in widely different projects. Adding a feature or fixing a bug needs to take all this into account.
- Don't break things, try to fix a bug without changing the API or when adding a feature make sure to add, not remove stuff. If you do need to break things, get in touch by reporting an [issue](https://github.com/itinero/routing/issues).

We also ask all contributors to sign a [contributor agreement](https://docs.google.com/forms/d/e/1FAIpQLSebPyLfaneaDUXXkNaMC8U7UfmW-IORpjiOcBotePtuuy5W6g/viewform). We do this to be able to change the license afterwards without having to contact everyone individually. Don't worry, we won't be closing off any of the code and what's there now will stay that way. It is possible that we change in the future from for example GPLv2 to MIT, or even public domain. The agreement guarantees your code stays open and you'll be given credit.

### Practicalities

To build this project from source you need to install .NET core, go to the [dot.net website](dot.net) to install it for your platform. You can then use the build scripts to build the project and/or run the tests.

There are two test projects:

- Itinero.Test: These are the unittests, add things here when you add new features, make sure things are test.
- Itinero.Test.Functional: These are functional tests to test the main features of Itinero in real-world scenario's. When you run these all data get's downloaded.

Usually development is done using VS code but you can use any of the following:

- Visual Studio Code
- Visual Studio (Windows)
- Visual Studio (Mac)
- Jetbrains Rider
- (any text editor)