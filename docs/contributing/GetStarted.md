# Contribution to Saturn

You can contribute to the Saturn project with issues and pull requests. Simply filing issues or utilizing the `Support` channel in [Saturn's Discord](https://discord.gg/Saturn) is a great way to contribute. Contributing implementations is also greatly appreciated and will earn you the `Contributor` role on [Saturn's Discord](https://discord.gg/Saturn).

## Reporting Issues

We always welcome bug reports, API issues, or feature requests. Please file an issue on [Saturn's GitHub](https://github.com/Tamely/SaturnSwapper) if you want to contribute in this way.

* If your issue is a security vulnerability, please to not file it as a normal issue. Instead, privately DM a holder of either the Contributor or Admin role in [Saturn's Discord](https://discord.gg/Saturn) and ask them to fix it or report it to Tamely.
    - Usually, you will get a response from them within 24 hours, but if - for whatever reason - you don't, please contact Tamely#6469 directly on Discord and report it to him.

## Contributing Changes

Project maintainers will merge changes that improve the project and make it more user-friendly.

Contributions must also satisfy the other publish guidelines defined in this document.

### DOs and DON'Ts

Do:
* **DO** follow our [coding style](docs/contributing/CodingStyle.md)
* **DO** test your implementation before making a pull request
* **DO** mark an line with `// TODO: ` or `// FIXME: ` to indicate a task that needs to be done and you weren't able to finish it then let us know in the pull request thread
* **DO** post in [Saturn's Discord](https://discord.gg/Saturn) about your contribution and how it helps the project as we might be able to share insight with you on how to improve or better align your idea with the current project

Don't:
* **DON'T** make pull requests for style changes
* **DON'T** include multiple implementations in the same pull request
* **DON'T** commit code you didn't write. If you are going to use someone else's code, please either rewrite it and comment `// Modified from {url}` next to the function signature or ask for permission to use it

### Suggested Workflow

We use and recommend the following workflow:
1. Create an issue for your work on this repo or post about it in [Saturn's Discord](https://discord.gg/Saturn).
    - You can skip this step for trivial changes.
    - Reuse an existing issue on the topic if you can.
    - Get agreement from project maintainers that your issue is worth working on or that no one else is working on it.
    - Clearly state that you are going to be implementing it if you are not just filing a bug report.
2. Create a personal fork of this repo (if you don't have one already).
3. In your fork, create a branch off of **master** (`git checkout -b <branch-name>`).
    - Name the branch so that it clearly indicates your intentions, such as `broken-beta-detection-1`.
    - Branches are useful since they isolate your work from incoming changes from upstream. They also enable you to create multiple pull requests from the same fork.
4. Make and commit your changes to the branch.
    - [Workflow Instructions](docs/contributing/BuildingAndRunning.md) explains how to build and test your changes.
5. Build the repository with your changes.
    - Make sure that the builds are clean.
    - Make sure that everything implemented works as expected.
6. Create a pull request against the Tamely/SaturnSwapper repository's **master** branch.
    - State in the description what issue or improvement you are addressing.
7. Wait for feedback or approval of your changes from the project maintainers.
8. When the project maintainers have approved, your pull request will be merged.
    - The next official release will include your change.
    - You can delete the branch you used to make your changes.