# ezBuild
Build manager package for Unity. pretty much just for my personal use, but if it's handy to anyone that'd be nice

currently supported build targets:

```
WebGL
OSX64
PC64
Linux64
```

these are extremely easy to add to, so i'll probably add more as i go along.

# installation
download. open in unity. go to Assets / export package. export it into your project. now you're cookin w packages!!

# use
- create a gameObject and attach the "EzBuild" component to it
  - in the inspector, choose a folder to build into
- create child gameObjects and attach "EzBuildTarget" components to them

that's it! you can toggle individual build settings on the targets & build them individually in the inspector, or (main advantage of this package) you can build all of your targets at once with "build all" on the parent object.

it'll also automatically zip your builds if you check "zip"


