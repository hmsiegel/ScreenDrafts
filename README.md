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

## Technologies Used

### Frontend
- [Next.js](https://nextjs.org/) - Frontend framework
- [React](https://reactjs.org/) - JavaScript library for building user interfaces
- [TypeScript](https://www.typescriptlang.org/) - Typed superset of JavaScript
- [Tailwind CSS](https://tailwindcss.com/) - Utility-first CSS framework
- [Keycloakify](https://keycloakify.dev/) - Library for building Keycloak themes

### Backend
- [C#](https://docs.microsoft.com/en-us/dotnet/csharp/) - Programming language for backend development
- [.NET](https://dotnet.microsoft.com/) - Framework for building and running applications
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - Object-relational mapper for .NET
- [PostgreSQL](https://www.postgresql.org/) - Open-source relational database management system
- [Docker](https://www.docker.com/) - Platform for developing, shipping, and running applications in containers
- [Redis](https://redis.io/) - In-memory data structure store, used as a database, cache, and message broker
- [Keycloak](https://www.keycloak.org/) - Open-source identity and access management solution
- [RabbitMQ](https://www.rabbitmq.com/) - Open-source message broker software
- [Ardalis.GuardClauses](https://github.com/ardalis/GuardClauses) - Library for guard clauses in C#
- [Ardalis.SmartEnum](https://github.com/ardalis/SmartEnum) - Library for creating smart enums in C#
- [Bogus](https://github.com/bchavez/Bogus) - Library for generating fake data
- [FluentValidation](https://fluentvalidation.net/) - Library for building strongly-typed validation rules
- [MediatR](https://github.com/jbogard/MediatR) - Library for implementing the mediator pattern
- [FastEndpoints](https://fast-endpoints.com/) - Library for building fast and efficient endpoints in .NET
- [ImdbApi](https://tv-api.com/) - API for accessing IMDB data
- [OmdbApi](https://www.omdbapi.com/) - API for accessing movie and TV show data
- [Serilog](https://serilog.net/) - Diagnostic logging library for .NET
- [MassTransit](https://masstransit-project.com/) - Distributed application framework for .NET
- [YARP] (https://microsoft.github.io/yarb/) - Reverse proxy for ASP.NET Core


## Author

- [@hmsiegel](https://www.github.com/hmsiegel)




[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/)


