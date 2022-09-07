# Planck

> Readme in progress, subject to change

## Usage

### From dotnet/C#

```cs
// dotnet new --install Planck.Application --name MyDesktopApp

using Planck;

// without dependency injection
var app = await PlanckApplication.StartAsync(new()
{
  SslOnly = true
});
// with dependency injection
var host = PlanckApplication.CreateHost(new()
{
  SslOnly = true
}).Build();
await host.StartAsync();
```


### From NPM/Yarn

```js
// npm install @planck/app

import { application } from '@planck/app';

const app = application({
  sslOnly: true,
});

app.start().then(() => {
  app.show();
  console.log('Hello from Planck');
});
```
