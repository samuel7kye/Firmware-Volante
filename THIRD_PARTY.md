# Componentes de terceiros

Volante Studio é uma interface independente. Não é um produto oficial da Logitech, Arduino ou EMC e não utiliza logotipos dessas marcas no painel.

A comunicação usa os seguintes componentes, incluídos no instalador completo:

- `EMC.dll` e `EMCHid.dll`: comunicação e comandos do EMC Utility Lite.
- `MaterialDesignColors.dll` e `MaterialDesignThemes.Wpf.dll`: dependências da distribuição existente, copiadas sem alteração para compatibilidade.

Esses arquivos são distribuídos dentro do instalador completo, sem modificação. Não recebem uma nova licença por estarem sendo usados pela interface e permanecem sujeitos aos termos dos respectivos titulares. Sua inclusão permite instalar o Volante Studio sem instalar o EMCLite separadamente. O código-fonte das bibliotecas não faz parte deste projeto.

O firmware `EMCLite0932.hex` e os instaladores fornecidos pelo usuário permanecem em backup local. Nenhum código de firmware ou código descompilado de terceiros foi incorporado ao código-fonte publicado.

A aparência foi criada com XAML e desenhos vetoriais próprios, a partir da referência visual fornecida pelo usuário.
