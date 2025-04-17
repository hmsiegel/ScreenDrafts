<p align="center">
  <img src="./res/screen-drafts-small.jpg" alt="Logo">
</p>

<p align="center">
	<img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/hmsiegel/ScreenDrafts/build.yml">
	<img alt="Dynamic XML Badge" src="https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fhmsiegel%2FScreenDrafts%2Frefs%2Fheads%2Fmain%2Fsrc%2Fscreendrafts.api%2FDirectory.Build.props&query=%2F%2FTargetFramework%5B1%5D&logo=.net&label=target&color=%23512bd4">
	<img alt="Dynamic JSON Badge" src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fhmsiegel%2FScreenDrafts%2Frefs%2Fheads%2Fmain%2Fsrc%2Fscreendrafts.ui%2Fpackage.json&query=%24.dependencies.next&logo=nextdotjs&label=NextJS">
</p>



<h1 align="center">Screen Drafts</h1>
<p align="center">Screen Drafts is a podcast where experts and enthusiasts competitively collaborate in the creation of screen-centric "Best Of" lists.</p>
<p align="center">This website application is being written to assist with the game format of the podcast. It will also act as a compendium to the <a href="https://screendrafts.fandom.com/wiki/Screen_Drafts">Screen Drafts Wiki</a></p>
<p align="center">It will be written in two parts: the front-end will use <a href="https://nextjs.org">Next.JS</a>, while the back-end API will use <a href="https://github.com/dotnet/core">.NET</a>.</p>

<h2 align="center">Screenshots</h2>

<p align="center">
	<img src="./res/home-page.png" alt="Home Page">
	<img src="./res/guest-landing.png" alt="Guest Landing">
</p>

## Development Setup

### Configuration Files
Development configuration files (*.Development.json) are ignored by git to protect sensitive data. Follow these steps to set up your development environment:

1. IMDB Integration Setup:
   - Locate the template file at `src/screendrafts.api/src/api/ScreenDrafts.Web/modules.integrations.Development.template.json`
   - Create a copy named `modules.integrations.Development.json` in the same directory
   - Replace the `{{IMDB_API_KEY}}` placeholder with your IMDB API key

To obtain an IMDB API key, visit [IMDB API Documentation](https://imdb-api.com/api)

Note: Other *.Development.json files follow a similar pattern. Check the templates provided for each module.

## Author

- [@hmsiegel](https://www.github.com/hmsiegel)




[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/)


