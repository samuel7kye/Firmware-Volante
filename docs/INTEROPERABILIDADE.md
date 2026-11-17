# Integração USB e bibliotecas

A instalação examinada expõe `EMC.MainViewModel`, `EMCHid.HidUtility` e comandos WPF. Sua identificação visual de software era `0.934`. O firmware fornecido em separado chama-se `EMCLite0932.hex`; não foi alterado nem gravado.

As bibliotecas necessárias acompanham o instalador; não é necessário instalar o EMCLite. A interface foi escrita do zero. Não recompila nem modifica as bibliotecas originais. A inicialização acontece no dispatcher WPF depois da criação da janela. Conexão é lida de `HidUtil.ConnectionStatus`; comandos só ficam disponíveis quando a própria biblioteca habilita `mainWindow`.

## Escalas e comandos

| Ajuste | API original | Conversão |
|---|---|---|
| Ângulo | `Angle` | 30–1080, em graus |
| Intensidade | `Maxforce` | 0–100 |
| Força mínima | `Minforce` | 0–100 |
| Amortecimento | `GetDamper` | UI 0–100 → API 0–200 |
| Batente | `GetSoftlock` | 0–100 |
| Atrito | `GetFriction` | UI 0–100 → API 0–200 |
| Inércia | `GetInertia` | UI 0–100 → API 0–200 |
| Encoder | `GetSetCpr` | Inteiro 1–65535 como texto |
| Ponte H | `HbridgeMode` | Mesmas quatro strings do original |
| Entradas | `DisableClutch`, `DisableHandbrake` | Booleanos; “Manter” não envia nada |
| Centro | `DoSetCenter` | `ICommand.Execute(null)` |
| Pedais | `DoPedalCommand` | Parâmetros abaixo, com capitalização original |

Pedais: `Gasmin`, `Gasmax`, `BrakeMin`, `BrakeMax`, `ClutchMin`, `ClutchMax`, `HandbrakeMin`, `HandbrakeMax`.

As faixas de leitura reproduzem os controles do executável original: `Throttle` de −32767 a 32767; `Brake`, `Clutch` e `Handbrake` de 0 a 1024. O ângulo visual vem de `RotateAngle`. A visualização é suavizada; os dados enviados aos jogos não são interceptados.

## Confirmação de transporte

O construtor de pacotes da biblioteca foi exercitado **em memória**, vinculando seu helper USB a um VID/PID reservado inexistente (`FFFF:FFFF`). Não foi executado um teste físico. A biblioteca gera também pacotes ociosos `FF…`; esses pacotes não podem confirmar uma alteração.

| Ação | Prefixo observado | Valor relevante |
|---|---|---|
| Ângulo 900° | `EE 02` | Bytes 2–3: `03 84` |
| CPR 4800 | `EE 01` | Bytes 2–3: `12 C0` |
| Intensidade 30 | `EE 27` | Byte 6: `1E` |
| Força mínima 0 | `EE 26` | Byte 6: `00` |
| Amortecimento 100 | `EE 20` | Byte 4: `64` |
| Batente 50 | `EE 06` | Byte 4: `32` |
| Atrito 100 | `EE 21` | Byte 4: `64` |
| Inércia 100 | `EE 22` | Byte 4: `64` |
| Centro | `EE 07` | Sem valor |
| Pedais | `EE 09` a `EE 10` | Na ordem dos parâmetros acima |
| Desativar/ativar embreagem | `90 0D` / `90 0E` | Sem valor |
| Desativar/ativar freio de mão | `90 0F` / `90 10` | Sem valor |
| 1PWM+1DIR / 1PWM+2DIR | `90 11` / `90 12` | Sem valor |
| 2PWM+1DIR / 2PWM+2DIR | `90 13` / `90 14` | Sem valor |

`TransferExpectation` só reconhece a confirmação: não gera ou transmite pacotes. A geração e o transporte permanecem a cargo de EMC/EMCHid. O envio aguarda `RaisePacketSentEvent` com `TransferSuccessful=true` e conteúdo correspondente; timeout ou desconexão mantêm pendentes os ajustes sem confirmação. Isso não comprova persistência em EEPROM.

## Teste físico pendente

Quando houver um volante conectado: comparar eixos no aplicativo e em `joy.cpl`; confirmar centralização e calibração; aplicar um ajuste pequeno; conferir limites e force feedback em um jogo; desconectar/reconectar; e verificar quais ajustes persistem após reiniciar a placa. Nenhuma dessas etapas foi declarada aprovada sem o hardware.
