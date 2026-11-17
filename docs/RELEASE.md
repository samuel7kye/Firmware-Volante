# Volante Studio 1.0.2

Instalador completo para Windows 10 e 11, em português. Inclui as bibliotecas necessárias: não exige instalação separada do EMC Utility Lite.

## Como usar

1. Baixe `VolanteStudio-Setup-1.0.2.exe` nos anexos da release.
2. Abra o arquivo e siga as instruções.
3. Abra o Volante Studio pelo menu Iniciar ou pelo atalho.
4. Conecte seu Leonardo já preparado com firmware compatível, ou use **Explorar demonstração**.

A instalação é por usuário, sem necessidade de senha de administrador. Permite escolher a pasta, oferece atalho na área de trabalho e registra o desinstalador no Windows. Perfis são preservados em atualizações e na desinstalação.

Requer .NET Framework 4.6.1 ou superior. O instalador verifica esse requisito. O executável do projeto ainda não possui assinatura digital de editor.

Esta versão corrige a execução da comunicação USB para 32 bits e a leitura da direção quando a escala interna da biblioteca ainda está zerada. Conexão e movimento da direção foram verificados com firmware 0.932. Pedais, force feedback, reconexão física e jogos permanecem pendentes. Não grava firmware nem instala drivers.

## Arquivos para anexar à release

- `release/VolanteStudio-Setup-1.0.2.exe`
- `release/VolanteStudio-Setup-1.0.2.exe.sha256`

O segundo arquivo permite conferir a integridade do download. Não é necessário para instalar.

## Gerar outro instalador

Compile o aplicativo e obtenha as quatro bibliotecas de comunicação da distribuição utilizada no projeto. Com Inno Setup 7 instalado:

```powershell
.\build.ps1 -OriginalPath 'C:\caminho\Volante Studio\compat' -Test
.\build-installer.ps1 -IsccPath 'C:\caminho\Inno Setup 7\ISCC.exe'
```

O empacotamento falha se faltar algum componente. Não inclui perfis, logs, firmware, instaladores antigos nem backups. O processo de CI compila e testa a interface; o instalador completo é produzido com as bibliotecas presentes localmente.
