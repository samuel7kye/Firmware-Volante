# Validação do instalador 1.0.0

Compilado com Inno Setup 7.1.1 e testado no Windows em uma pasta isolada, sem modificar a instalação original do EMCLite.

| Verificação | Resultado |
|---|---|
| Instalação silenciosa completa | Aprovada, retorno 0 |
| EXE, configuração e quatro DLLs instalados | Hashes idênticos aos arquivos de entrada |
| Atalho do menu Iniciar | Criado |
| Registro de desinstalação do usuário atual | Criado |
| Inicialização da comunicação | Aprovada, sem volante conectado |
| Origem de EMC.dll e EMCHid.dll carregadas | Pasta compat da instalação de teste; não Program Files/EMCLite |
| Reinstalação sobre a mesma pasta | Aprovada, perfil de teste preservado sem alteração |
| Desinstalação | Removeu programa, atalho e registro |
| Dados do usuário após desinstalar | Perfil preservado sem alteração |

O teste da comunicação utilizou o executável instalado e verificou os caminhos das bibliotecas carregadas. Nenhum dispositivo USB recebeu comandos. O teste físico do volante permanece pendente.

O instalador não inclui dados pessoais, perfis de teste, logs nem firmware. Não há assinatura digital de editor no executável gerado. O arquivo SHA-256 acompanha o instalador para conferência de integridade.
