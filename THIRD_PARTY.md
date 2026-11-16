# Componentes de terceiros

Volante Studio é uma interface independente. Não é um produto oficial da Logitech, Arduino ou EMC e não utiliza logotipos dessas marcas no painel.

A integração local depende de componentes fornecidos pela instalação do usuário:

- `EMC.dll` e `EMCHid.dll`: comunicação e comandos do EMC Utility Lite.
- `MaterialDesignColors.dll` e `MaterialDesignThemes.Wpf.dll`: dependências da distribuição existente, copiadas sem alteração para compatibilidade.

Esses arquivos não são incluídos neste repositório nem recebem uma nova licença por estarem sendo usados pela interface. Permanecem sujeitos aos termos dos respectivos titulares. O pacote local preparado para o proprietário da instalação usa cópias dos arquivos que já estavam em sua máquina.

O firmware `EMCLite0932.hex` e os instaladores fornecidos pelo usuário permanecem em backup local. Nenhum código de firmware ou código descompilado de terceiros foi incorporado ao código-fonte publicado.

A aparência foi criada com XAML e desenhos vetoriais próprios, a partir da referência visual fornecida pelo usuário.
