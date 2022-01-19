# Azure KeyVault (AKV)

O **Azure Key Vault** é serviço de nuvem para armazenar e acessar segredos de maneira segura. Um segredo (ou *secret*) é qualquer coisa a qual você queira controlar rigidamente o acesso, como chaves de **API**, senhas, certificados ou chaves criptográficas.

Para usar o **Azure KeyVault (AKV)** em um projeto, precisamos seguir os passos abaixo.

# 1. Criar um AKV

Precisamos ir até o portal Azure ([http://www.portal.azure.com](http://portal.azure.com)), acessar **Cofres de Chaves**.

![Untitled](assets/Untitled.png)

![Untitled](assets/Untitled%201.png)

Clicamos em **+ Criar** para criar um novo cofre. Para criar um novo cofre precisamos da assinatura equivalente e do grupo de recursos (ou podemos criar um novo) ao qual o cofre pertence. Além disso, também é necessário indicar um nome para o cofre (nesse caso nomeamos o cofre para *LearingAKV*).

![Untitled](assets/Untitled%202.png)

Clicando em **Revisar + criar** ele fará a validação e em seguida aparecerá uma tela para revisar e criar. Finalmente clique em **criar**.

![Untitled](assets/Untitled%203.png)

Irá demorar um pouco para que o cofre seja implantado. Após a implantação vamos acessar o recurso para cadastrar uma nova *secret*.

# 2. Criando uma Secret (Segredo)

Acessando o recurso, vá até **Segredos** no menu lateral, e clique em **Gerar/importar** para criar um novo segredo.

![Untitled](assets/Untitled%204.png)

Dê um *nome* e o *valor* da secret. Nessa caso, iremos guardar uma *connection string*: 

```csharp
Server=localhost;Database=TesteDb;User Id=sa;Password=Ewmg@1713;Encrypt=YES;TrustServerCertificate=True;MultipleActiveResultSets=true;
```

![Untitled](assets/Untitled%205.png)

Após clicar em salvar, nossa *secret* estará cadastrada.

# 3. Obtendo as Key no .NET6

## 3.1. Configurações na `appsettings.json`.

No arquivo de `appsettings.json` precisamos inserir algumas informações que serão utilizadas para acessar o AKV. Veja abaixo como serão inseridas essas informações:

```csharp
{
  "KeyVaultConfig": {
    "KVUrl": "https://learningakv.vault.azure.net/",
    "TenantId": "587fba4e-fb52-44a5-aa8a-8f5d63cd5df6",
    "ClientId": "721a4dc4-5d51-46ce-87b9-04ec3afd8993",
    "ClientSecretId": "JMS7Q~0gK3WwPlS3DyOEHXS2XPqYcTXUIInVk"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "AllowedHosts": "*"
}
```

Falaremos a partir de agora o que são as informações necessárias e como a obter:

### O `KVUrl` (*Url* do *KeyVault*)

O `KVUrl` é a *url* da sua ***KeyVault*** cadastrada. Ela pode ser encontrada na página de overview do recurso:

![Untitled](assets/Untitled%206.png)

### O `TenantId`, `ClientId` e `ClientSecretId`

Tanto o `TenantId` , `ClientId` e `ClientSecretId` são *id’s* da sua aplicação registrada na Azure. Para registrar sua aplicação na Azure, devemos ir até **Registro de Aplicativos** (ou *app registration*), e clicar em **+ Novo registro**.

![Untitled](assets/Untitled%207.png)

![Untitled](assets/Untitled%208.png)

Precisamos então dar um nome ao aplicativo e registrar:

![Untitled](assets/Untitled%209.png)

No *overview* do recurso conterá duas das informações necessárias para inserir no *json*:

![Untitled](assets/Untitled%2010.png)

- O **ID do aplicativo (cliente)** é o que deve ser inserido em `ClientId`;
- O **ID do diretório (locatário)** é o deve ser inserido em `TenantId`;

A última é o `ClientSecretId` que é a *secret* criada para aquela aplicação. Para criar ou acessar essa *secret* iremos no menu lateral até **Certificados e Segredos** (*certificates & secrets*) e clicar em **Novo segredo do cliente** (aba segredos do cliente):

![Untitled](assets/Untitled%2011.png)

Dar um nome a *secret* e adicionar:

![Untitled](assets/Untitled%2012.png)

Ao adicionar, nossa secret irá aparecer e precisaremos do **Valor** dela. Esse valor é o `ClientSecretId`:

![Untitled](assets/Untitled%2013.png)

## 3.2. Dando permissão a aplicação para acessar as *secrets*

Precisamos ainda dar permissão a aplicação registrada na Azure para acessar as *secrets*. Para isso acessamos o AKV criado, na aba **Políticas de Acesso** (ou *access policies*) clicamos em **+ Add Access Policy**:

![Untitled](assets/Untitled%2014.png)

![Untitled](assets/Untitled%2015.png)

- A primeira opção, colocamos em **Gerenciamento de Segredos**;
- Em permissões do segredo devemos indicar as permissões de acesso a *secret*. Iremos colocar somente *get* e *list*;
- Finalmente precisamos **Selecionar entidade de segurança**: iremos pesquisar pelo nosso app registrado e clicar em **Selecionar**:

![Untitled](assets/Untitled%2016.png)

E então clicamos em **Adicionar** e o nosso *app* estará registrado para receber as *secrets*:

![Untitled](assets/Untitled%2017.png)

![Untitled](assets/Untitled%2018.png)

Não esqueça de **Salvar** antes de sair como mostra no *print* acima.

## 3.3. Configurações na classe `program.cs`.

Para obter as Keys através do .NET 6, iremos precisar de três pacotes:

![Untitled](assets/Untitled%2019.png)

- `Azure.Identity`;
- `Azure.Security.KeyVault.Secrets`;
- `Azure.Extensions.AspNetCore.Configuration.Secrets`.

Agora, na classe `program.cs` devemos configurar o acesso ao AKV:

```csharp
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    IConfiguration configuration = config.Build();

    string kvURL = configuration["KeyVaultConfig:KVUrl"];
    string tenantId = configuration["KeyVaultConfig:TenantId"];
    string clientId = configuration["KeyVaultConfig:ClientId"];
    string clientSecret = configuration["KeyVaultConfig:ClientSecretId"];

    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    var client = new SecretClient(new Uri(kvURL), credential);
    config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
});
```

Em `Host`, usamos `ConfigureAppConfiguration` onde seu argumento é uma função `(context, config)`. Dentro dele:

- Criamos o objeto do tipo `IConfiguration` a partir de `config.Build()` com as informações do `appsettings.json`;
- Obtemos todos os dados;
- Criamos uma credencial com `ClientSecretCredential`, informando a `tenantId`, `clientId`, `clientSecret`;
- Fazemos o acesso ao **AKV** com `SecretClient`, informando o *url* `kvURL` e a credencial;
- Finalmente injetamos ao config os valores recebidos do **AKV**;

# 4. Acessando a *secret* do programa

No código do passo anterior, injetamos todas as *secrets* no arquivo de configuração `appsettings.json`. Mas qual o nome para acessarmos? quando cadastramos a nossa *secret* no AKV nós atribuímos um nome a ela:

![Untitled](assets/Untitled%2020.png)

Onde o acesso é feito em `ConnectionStrings` no seu atributo `SqlServerConnectionStrings`. Com os dois traços `--` colocamos a hierarquia dos atributos *json*. Dessa forma, podemos acessar usando:

```csharp
builder.Configuration.GetConnectionString("SqlServerConnectionStrings")
```

Ou:

```csharp
builder.Configuration["ConnectionStrings:SqlServerConnectionStrings"]
```

Ou:

```csharp
builder.Configuration.GetSection("ConnectionStrings").GetSection("SqlServerConnectionStrings").Value
```

> 💡 **Referência**:
> José Ponto Cruz: [https://www.youtube.com/watch?v=FlvSe-prQ-g&ab_channel=JoséAsync](https://www.youtube.com/watch?v=FlvSe-prQ-g&ab_channel=Jos%C3%A9Async)