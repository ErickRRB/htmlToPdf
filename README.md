# Convertidor de HTML a PDF

Este proyecto es una aplicación para convertir archivos HTML a PDF utilizando Windows Forms y el control WebView2 de Microsoft. Está diseñada para ser ejecutada desde la línea de comandos con opciones configurables.

## Características

- **Conversión de HTML a PDF**: Utiliza WebView2 para renderizar HTML y exportarlo a un archivo PDF.
- **Interfaz oculta**: Se ejecuta en segundo plano sin mostrar una interfaz visible.
- **Manejo de errores**: Los errores se registran en un archivo `error.log`.
- **Ruta de salida opcional**: Si no se especifica una ruta de salida, se utiliza la misma que la del archivo de entrada con extensión `.pdf`.

## Requisitos

- **.NET 8.0** o superior
- **Windows** con soporte para aplicaciones de escritorio
- **WebView2 Runtime** instalado en el sistema

## Instalación

1. Clona este repositorio:
git clone https://github.com/tu-usuario/nombre-del-repo.git
cd nombre-del-repo

2. Restaura las dependencias:
dotnet restore

3. Compila la aplicación:
dotnet build

# Uso
Ejecuta el comando con las siguientes opciones:

ConvertidorHTMLaPDF --entrada "ruta/al/archivo.html" [--salida "ruta/al/archivo.pdf"]
--entrada: (Requerido) Ruta del archivo HTML de entrada.
--salida: (Opcional) Ruta del archivo PDF de salida. Si no se especifica, se utiliza la misma ruta que la de entrada con extensión .pdf.

# Ejemplo:
ConvertidorHTMLaPDF --entrada "documento.html" --salida "documento.pdf"