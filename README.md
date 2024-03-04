# ConvertShell (ru)
## API для конвертации файлов
Проект развернут по адресу: [http://158.160.147.26/swagger/index.html](http://158.160.147.26/swagger/index.html). В качестве облачной платформы был взят Yandex Cloud
  
## Для запуска проекта требуется:
- ASP.NET 7.0
- .NET SDK 7.0
## Установка
- Клонировать проект:
```
git clone https://github.com/Folonder/ConvertShell.git
```
- Установка зависимостей:
```
dotnet restore "ConvertShell\ConvertShell.csproj"
```
- Компиляция проекта:
```
dotnet build "ConvertShell\ConvertShell.csproj"
```
## Запуск
- Запуск проекта:
```
cd .\ConvertShell\
```
```
dotnet run "ConvertShell.csproj"
```
- Приложение запускается на 7088 и 5088 для https и http соответственно.
## Как работать с приложением
### Конвертация
- Перейдите по [https://localhost:7088/swagger/index.html](https://localhost:7088/swagger/index.html).
- Выберите нужный endpoint.

## Использованные технологии
- Polly - библиотека для повторных попыток вызова HTTP
- xUnit - фреймворк для тестирования приложения
- Moq - фреймворк для подмены зависимостей при тестировании
- WireMock - фреймворк для подмены сервера при тестировании


# ConvertShell (en)
## API for file conversion
The project is deployed at: [http://158.160.147.26/swagger/index.html ](http://158.160.147.26/swagger/index.html ). Yandex Cloud was taken as a cloud platform

## To launch the project, it is required:
- ASP.NET 7.0
- .NET SDK 7.0
## Installation
- Clone a project:
```
git clone https://github.com/Folonder/ConvertShell.git
```
- Installing dependencies:
```
dotnet restore "ConvertShell\ConvertShell.csproj"
```
- Compilation of the project:
```
dotnet build "ConvertShell\ConvertShell.csproj"
```
## Launch
- Starting the project:
```
cd .\ConvertShell\
```
```
dotnet run "ConvertShell.csproj"
```
- The application runs on 7088 and 5088 for https and http, respectively.
## How to work with the application
### Conversion
- Go to [https://localhost:7088/swagger/index.html ](https://localhost:7088/swagger/index.html ).
- Select the desired endpoint.

## Used technologies
- Polly - library for repeated HTTP call attempts
- xUnit - framework for testing the application
- Moq is a framework for substituting dependencies during testing
- WireMock is a framework for server substitution during testing
