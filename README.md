## Card Registry API
Uma API para cadastro e consulta de números de cartão completo, com autenticação JWT, logs via Serilog e criptografia de dados sensíveis.

---
## Descrição do projeto
O Card Registry API é uma API simples, mas segura. Ela permite:

* Cadastro de usuários com senha segura (hash PBKDF2 + SHA256).

* Autenticação via JWT para uso dos endpoints.

* Cadastro de cartões individualmente ou através de arquivos TXT.

* Consulta de cartões pelo número completo.

* Logs de todas as requisições usando Serilog.

* O foco principal do projeto foi a segurança, garantindo que dados sensíveis como números de cartão e senhas fossem armazenados de forma segura.

---
## Tecnologias utilizadas

* Linguagem: C#

* Framework: .NET 9

* Banco de dados: SQL Server (Docker)

* ORM: Entity Framework Core

* Autenticação: JWT

* Logs: Serilog

* Arquitetura: DDD (Domain-Driven Design)

* Testes: Xunit

* Hash de cartões: PBKDF2 + SHA256

---
## Endpoints principais

Usuario

POST /api/auth/register → Cadastro de usuário

POST /api/auth/login → Login e geração de token JWT

--

Cartao (Todos os endpoints abaixo exigem autenticação JWT no header)

GET /api/cartao/obterPorNumero/{cardNumber} → Consulta um cartão pelo número completo

POST /api/cartao/cadastraManual → Cadastra um cartão manualmente

POST /api/cartao/cadastraArquivo → Cadastra cartões a partir de um arquivo TXT

---
# Como rodar o projeto

* Clone o repositório
* Suba o SQL Server via Docker (docker-compose up -d)
* No Console do Gerenciador de Pacote, rode o comando (Update-Database)
* Rode a API
* Acesse o Swagger
* Quando for testar a API, não precisa por a palavra "Bearer" no Swagger, basta apenas colar o token
  
---
## Logs

Todas as requisições e retornos são logados pelo Serilog.

Logs padrão vão para o console e para o arquivo na pastas "logs"

---
## Testes
Testes unitários foram feitos usando Xunit.

---
## Observações

* A API não permite salvar cartões duplicados (verificação pelo hash completo).

* A segurança foi priorizada: senhas e cartões são hasheados antes de armazenar no banco.

* O JWT expira após 60 minutos e precisa ser enviado nos headers para qualquer operação de cartão.

* Usei mapeamento manual entre entidades e DTOs por ser mais rápido e performático.

* Optei por utilizar Repositórios e Services com Interfaces para separar responsabilidades, facilitar a manutenção, permitir a troca de implementações sem impactar o restante do sistema.

* Utilizei padrões de commit como (fix, feat, refactor, test e chore)

---
<img width="1691" height="212" alt="image" src="https://github.com/user-attachments/assets/e8b0b11c-40b2-45b0-a9e4-713a3df8156d" />
