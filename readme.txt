1.Откройте SQL Server Management Studio (SSMS) или Visual Studio и выполните скрипт database.sql

2. В проекте _5Elem.API создайте файл appsettings.json (если его нет) и запишите в него следующее:
{
  "ConnectionStrings": {
    "DefaultConnection": "ВАША_СТРОКА_ПОДКЛЮЧЕНИЯ"
  },
  "JWT": {
    "Secret": "ВАШ_СЕКРЕТНЫЙ_КЛЮЧ"
  },
  "ImageKit": {
    "PublicKey": "ВАШ_PUBLIC_KEY",
    "PrivateKey": "ВАШ_PRIVATE_KEY",
    "UrlEndpoint": "ВАШ_URL_ENDPOINT"
  },
  "AllowedHosts": "*"
}

ВАША_СТРОКА_ПОДКЛЮЧЕНИЯ (например): Server=(localdb)\MSSQLLocalDB;Database=_5ElemCatalog;Trusted_Connection=True; (Название БД: _5ElemCatalog)

"ВАШ_СЕКРЕТНЫЙ_КЛЮЧ" нужно сгенерировать, например, в powershell так -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | % {[char]$_})

"ВАШ_PUBLIC_KEY", "ВАШ_PRIVATE_KEY", "ВАШ_URL_ENDPOINT" берутся с сайта https://imagekit.io/ (нужно зарегистрироваться)

3.В проекте _5Elem.Client создайте файл appsettings.json:

json
{
  "ApiSettings": {
    "BaseUrl": "ВАШ_URL"
  }
}