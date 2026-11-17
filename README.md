# Volante Studio

Aplicativo para configurar um volante Arduino Leonardo no Windows, com interface em português, visual escuro, controles azuis e volante animado.

**Autor: Samuel — [@samuel7kye](https://github.com/samuel7kye)**
Projeto acadêmico para a faculdade.

![Volante Studio em modo de demonstração](docs/preview.jpg)

## Instalação

**[Baixar o instalador completo (.exe)](https://github.com/samuel7kye/Firmware-Volante/raw/refs/heads/main/release/VolanteStudio-Setup-1.0.0.exe)**

1. Baixe e execute o instalador.
2. Siga as instruções em português.
3. Abra o Volante Studio pelo menu Iniciar ou pelo atalho da área de trabalho.

O instalador já inclui as bibliotecas necessárias. **Não é necessário instalar o EMCLite separadamente.** A instalação é feita para o usuário atual e pode ser removida pelas Configurações do Windows. Atualizações e desinstalação preservam os perfis salvos.

Requisitos: Windows 10 ou 11 e .NET Framework 4.6.1 ou superior. O instalador verifica esse requisito. O executável ainda não possui assinatura digital de editor.

## Como usar

Conecte pelo USB um Leonardo já preparado com firmware EMC compatível. Aguarde o indicador de conexão, ajuste os valores e clique em **Aplicar ao volante**. Em **Perfis**, salve configurações para reutilizar depois.

Sem o volante conectado, escolha **Explorar demonstração** para conhecer a interface. Nesse modo, os movimentos são simulados e nenhum ajuste é enviado ao dispositivo.

## Recursos

- Ângulo de rotação, centralização e visualização da direção.
- Intensidade, força mínima, amortecimento, batente, atrito e inércia.
- Leitura e calibração dos pedais.
- Configuração do encoder, ponte H e entradas opcionais.
- Perfis locais e acesso ao teste de botões do Windows.

## Compatibilidade e testes

O aplicativo utiliza as bibliotecas EMC incluídas no pacote. O Leonardo continua enviando os comandos aos jogos pelo firmware atual. O instalador não grava firmware nem instala drivers.

A interface, os 17 testes de software e a instalação, atualização e desinstalação foram verificados. **Os testes físicos de eixos, force feedback, reconexão e funcionamento em jogos ainda estão pendentes.**

## Organização do projeto

| Pasta ou arquivo | Finalidade |
|---|---|
| `src/` | Código-fonte da interface e comunicação |
| `tests/` | Testes automatizados |
| `installer/` | Definição do instalador e instruções incluídas nele |
| `release/` | Instalador completo e verificação SHA-256 |
| `docs/` | Prévia, documentação técnica e orientações de release |
| `build.ps1` | Compilação e testes do aplicativo |
| `build-installer.ps1` | Empacotamento do instalador |

Para desenvolver, execute `powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -Test`. A compilação e os testes da interface não exigem instalação do EMCLite. Para empacotar a comunicação, forneça ao build as bibliotecas que acompanham o pacote; veja [como gerar o instalador](docs/RELEASE.md).

Documentação: [integração USB](docs/INTEROPERABILIDADE.md), [testes do instalador](docs/TESTES-INSTALADOR.md) e [componentes de terceiros](THIRD_PARTY.md). A autoria do projeto não altera os créditos e os termos das bibliotecas utilizadas.
