# OWL

## Description

OWL is a system for reserving arrivals in a warehouse.

## Setup

Angular setup:

```
# install node at least v16.10
cd ClientApp
npm i
npm run build
```

For .NET install the VSCode extension: C#

During development use the watcher command:

```
npm run watch
```

## Note about Fish shell and nvm

When using fish shell, you need to put the version >= 16.10 of node into your path. You can do this here:

```
# .config/fish/conf.d/config.fish
set PATH ~/.nvm/versions/node/v16.13.0/bin $PATH
```

This is the version that .net will see. You can verify it works by running `bash` in your command output, then running `node` there.

## Deployment

DEPRECATED. TODO: update.

Update deployment:

```
cd /root
eval `ssh-agent -s`
ssh-add warehouses_git
cd omniopti-warehouses
git pull origin master
systemctl stop warehouses
dotnet publish -c Release --output ../publish /p:EnvironmentName=Production
if database structure changed: dotnet ef database update
systemctl start warehouses
```

For staging:

```
cd /root
eval `ssh-agent -s`
ssh-add ./warehouses_git
cd omniopti-warehouses
git pull origin master
systemctl stop warehouses-test
dotnet publish -c Release --output ../publish-test /p:EnvironmentName=Staging
if database structure changed:
set ASPNETCORE_ENVIRONMENT=Staging
dotnet ef database update

    systemctl start warehouses-test

```

## I18N

### Update localization

As a prerequisite, when writing code, add i18n flags to html files in ClientApp/src/app as instructed here https://angular.io/guide/i18n (potentially adding <ng-content></ng-content> around text)

#### Update translations

This is the process of updating translations.

First, we need to sync the translations with the code. In ClientApp/, run:

```bash
# slovenian
yarn run extract-i18n

# croatian
yarn run extract-i18n-hr
```

Now, take the files `ClientApp/src/locale/messages.hr.xlf` and `ClientApp/src/locale/messages.sl.xlf` and translate them.

Finally, paste them back into the project.

### Adding new language

1. make a new messages.[locale].xlf file like in Update localization instructions
2. In Warehouses.csproj add e.g.
   <Exec WorkingDirectory="$(SpaRoot)" Command="ng build --prod --output-path=dist-sl --i18n-file=src/locale/messages.sl.xlf --i18n-format=xlf --i18n-locale=sl" />
   also add the output-path to this line <DistFiles Include="$(SpaRoot)dist\**; $(SpaRoot)dist-sl\**; $(SpaRoot)dist-server\**" />

## Generating PDF files

Due to C# limitations, Python 3 is used to generate PDF files. For this you need to prepare the execution environment of the server as follows:

```
# run these with the user that is executing the C# server

pip3 install pdfkit # or pip install, depending on which package manager maintains which Python version
pip3 install pyvirtualdisplay
sudo apt-get install wkhtmltopdf
sudo apt-get install xvfb
```

Then, in appSettings.\*.json file, modify the "PythonPath" variable to point to the location where Python 3 is installed.

### Database

First make sure you have EF tool installed:

```
dotnet tool install --global dotnet-ef
```

Now you can run a migration. Run a migration only when server is not running (otherwise you get an error):

```
dotnet ef migrations add MigrationName
```
