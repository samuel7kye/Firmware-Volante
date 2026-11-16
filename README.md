# Volante Studio

Aplicativo nativo para Windows que renova a interface de configuração do volante Arduino Leonardo com EMC Utility Lite. Fundo preto com iluminação difusa, controles azuis, volante vetorial animado e interface em português.

![Painel do Volante Studio em demonstração](docs/preview.jpg)

## Abrir

Com o EMCLite original instalado, abra **[release/VolanteStudio.exe](release/VolanteStudio.exe)**. Mantenha o arquivo `.config` ao lado do executável. A pasta `release` pode ficar em Documentos.

O pacote portátil local também inclui uma cópia das bibliotecas do aplicativo já instalado. Nenhum instalador é necessário. Para outra máquina, mantenha a pasta `compat` junto ao executável ou instale a sua cópia legítima do EMCLite.

1. Feche o EMCLite antigo para evitar duas interfaces disputando a mesma conexão.
2. Abra o Volante Studio e conecte o Leonardo pelo USB.
3. Confira o indicador de conexão e a leitura dos eixos.
4. Prepare os ajustes e clique em **Aplicar ao volante**.
5. Use **Perfis** para salvar combinações neste computador.

Sem o volante, clique em **Explorar demonstração**. Os movimentos são identificados como simulados e todos os envios USB ficam bloqueados. A demonstração não é usada automaticamente quando o USB está ausente.

## O que está incluído

- Ângulo de 30° a 1080°, com atalhos para ângulos comuns.
- Intensidade, força mínima, amortecimento, batente, atrito e inércia.
- Leitura do volante, acelerador, freio, embreagem e freio de mão.
- Centralização e calibração de mínimo/máximo dos pedais.
- CPR do encoder, modo da ponte H e ativação das entradas opcionais.
- Perfis XML locais em `Data/Profiles`, sem envio automático ao carregar.
- Acesso ao painel de controladores do Windows para testar botões.
- Reconhecimento USB delegado ao componente original, sem drivers adicionais.
- Janela redimensionável com layout vetorial proporcional.

## Firmware e compatibilidade

Este repositório é o **aplicativo**, não um firmware novo. Não há gravador, comunicação com bootloader, mudança de descritor USB ou instalação de driver. O arquivo `EMCLite0932.hex` fornecido permanece intacto.

A comunicação usa as APIs públicas de `EMC.dll` e `EMCHid.dll` da instalação existente. Os jogos continuam recebendo o dispositivo HID diretamente do firmware do Leonardo; a nova interface não fica entre o volante e o jogo. A compatibilidade efetiva continua dependendo do firmware, da montagem e do jogo.

**Situação da validação:** build e 17 testes locais aprovados; navegação, demonstração, perfis e bloqueio de comandos sem dispositivo conferidos na aplicação aberta. A montagem dos comandos foi examinada em memória com a biblioteca original, isolada de hardware. **Leitura de eixos físicos, force feedback, reconexão e persistência no Leonardo ainda precisam de teste com o volante conectado.**

O aviso “comandos enviados” significa uma transferência USB bem-sucedida com o relatório, comando e valor esperados. Não representa confirmação de gravação em EEPROM. O aplicativo não inventa uma operação “salvar no dispositivo” que a API não fornece.

## Compilar

Requisitos: Windows com .NET Framework 4.6.1 ou superior e Windows PowerShell. Não exige Node, navegador, Visual Studio nem downloads de pacotes.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1 -Test
```

O resultado fica em `bin/VolanteStudio.exe`. Para usar uma cópia de backup do EMCLite:

```powershell
.\build.ps1 -OriginalPath 'C:\caminho\EMCLite-original' -Test
```

O script apenas lê a instalação original e copia suas bibliotecas para `bin/compat`. A ausência delas não impede compilar nem executar os testes de interface e validação; impede a comunicação real.

## Estrutura

| Local | Conteúdo |
|---|---|
| `src/MainWindow.xaml` | Layout, cores e controles |
| `src/WheelView.cs` | Desenho e rotação do volante |
| `src/App.cs` | Navegação, leitura, perfis e envio explícito |
| `src/EmcBridge.cs` | Integração com as bibliotecas originais |
| `src/TransferExpectation.cs` | Correspondência dos comprovantes de envio |
| `tests/Tests.cs` | Validação de escalas, perfis, interface e bloqueios |
| `docs/INTEROPERABILIDADE.md` | Mapeamento verificado da integração |
| `release/` | Executável da interface e configuração |

As bibliotecas de terceiros, o firmware e os backups pessoais não são publicados no Git. Consulte [THIRD_PARTY.md](THIRD_PARTY.md).
