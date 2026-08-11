"""Genera los iconos del sitio a partir de wwwroot/images/logo-patic.png.

El logo completo es ilegible por debajo de ~64px: los textos curvos "ESTETICA" y
"COSMETOLOGIA" se convierten en ruido. Este script arma una version simplificada
-- anillo con el degradado del original + la firma "Pati C" ampliada -- que
sobrevive a 16px. Los colores del anillo y el rosa del interior se muestrean del
logo real para no inventar la paleta.
"""

from PIL import Image, ImageDraw
import math
import os

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(RAIZ, "AestheticCenter.Web", "wwwroot")
SRC = os.path.join(OUT, "images", "logo-patic.png")

# Region de la firma dentro del logo original (medida sobre la densidad de trazos:
# arriba queda "ESTETICA", abajo "COSMETOLOGIA", y en el medio la firma).
FIRMA_BOX = (39, 95, 192, 158)

INTERIOR = (255, 224, 232)   # rosa del relleno, muestreado del original
TINTA = (26, 22, 24)         # tinta de la firma

# Render interno: 8x el tamano final mas grande, para bajar por LANCZOS.
SS = 2048


def cargar_original():
    im = Image.open(SRC).convert("RGBA")
    flat = Image.new("RGB", im.size, "white")
    flat.paste(im, mask=im.getchannel("A"))
    return flat


def muestrear_anillo(flat):
    """Devuelve 360 colores: para cada angulo, el pixel mas saturado del trazo."""
    w, h = flat.size
    px = flat.load()
    cx, cy = w / 2, (34 + 209) / 2
    colores = []
    for deg in range(360):
        a = math.radians(deg)
        best, bs = (255, 0, 128), -1
        for r10 in range(int(min(w, h) * 3.0), int(max(w, h) * 6.0)):
            r = r10 / 10
            x, y = int(cx + r * math.cos(a)), int(cy + r * math.sin(a))
            if not (0 <= x < w and 0 <= y < h):
                break
            R, G, B = px[x, y]
            s = max(R, G, B) - min(R, G, B)
            if s > bs:
                bs, best = s, (R, G, B)
        colores.append(best)
    # Suavizado circular: quita saltos de un grado a otro.
    suave = []
    for i in range(360):
        vec = [colores[(i + d) % 360] for d in range(-4, 5)]
        suave.append(tuple(sum(c[k] for c in vec) // len(vec) for k in range(3)))
    return suave


def extraer_firma(flat):
    """Recorta la firma y la devuelve como imagen RGBA con fondo transparente."""
    rec = flat.crop(FIRMA_BOX)
    # El fondo es el rosa del interior; la firma es tinta oscura. Una rampa entre
    # ambos da el alpha con el antialiasing del trazo. El corte alto va por encima
    # del rosa del fondo (lum ~237) para que el recorte no deje un halo rectangular.
    CLARO, OSCURO = 205, 45
    gris = rec.convert("L")
    alpha = gris.point(lambda p: max(0, min(255, int((CLARO - p) * 255 / (CLARO - OSCURO)))))
    firma = Image.new("RGBA", rec.size, TINTA + (0,))
    firma.putalpha(alpha)
    return firma


def construir_icono(colores_anillo, firma):
    """Icono en RGBA: fuera del disco queda transparente, para poder usarlo tal
    cual sobre el fondo del sitio y aplanarlo sobre blanco donde haga falta."""
    lienzo = Image.new("RGBA", (SS, SS), (255, 255, 255, 0))
    draw = ImageDraw.Draw(lienzo)

    diam = SS * 0.96                  # el disco casi llena el cuadro
    grosor = SS * 0.085               # anillo grueso: a 16px tiene que leerse el color
    m = (SS - diam) / 2

    hueco = SS * 0.022                # aro blanco entre el anillo y el relleno

    def disco(radio, color):
        draw.ellipse(
            [SS / 2 - radio, SS / 2 - radio, SS / 2 + radio, SS / 2 + radio], fill=color
        )

    # Base blanca opaca bajo el anillo -- si no, el aro entre el anillo y el
    # relleno rosa saldria transparente en vez de blanco.
    disco(diam / 2 - grosor / 2, (255, 255, 255, 255))
    r_int = diam / 2 - grosor - hueco
    disco(r_int, INTERIOR + (255,))

    # Anillo con degradado conico: un arco por grado, con solape para que no se
    # vean costuras entre segmentos.
    caja = [m + grosor / 2, m + grosor / 2, SS - m - grosor / 2, SS - m - grosor / 2]
    for deg in range(360):
        draw.arc(caja, deg - 0.5, deg + 1.5, fill=colores_anillo[deg], width=int(grosor))

    # Firma centrada, ocupando el 60% del ancho.
    ancho = int(SS * 0.60)
    alto = int(ancho * firma.height / firma.width)
    f = firma.resize((ancho, alto), Image.LANCZOS)
    lienzo.paste(f, ((SS - ancho) // 2, (SS - alto) // 2), f)
    return lienzo


def main():
    flat = cargar_original()
    icono = construir_icono(muestrear_anillo(flat), extraer_firma(flat))

    def png(size, name, opaco=False):
        path = os.path.join(OUT, name)
        img = icono.resize((size, size), Image.LANCZOS)
        if opaco:
            fondo = Image.new("RGB", img.size, "white")
            fondo.paste(img, mask=img.getchannel("A"))
            img = fondo
        img.save(path, "PNG", optimize=True)
        print("->", name)

    # Google exige un favicon cuadrado de al menos 48px (idealmente multiplo de 48).
    png(48, "favicon-48x48.png")
    png(96, "favicon-96x96.png")
    # iOS y Android no respetan la transparencia: la rellenan de negro.
    png(180, "apple-touch-icon.png", opaco=True)
    png(192, "icon-192.png", opaco=True)
    png(512, "icon-512.png", opaco=True)
    # Version para el header y las pantallas de login: se usa sobre el fondo nude.
    png(256, os.path.join("images", "logo-icon.png"))

    ico = os.path.join(OUT, "favicon.ico")
    icono.resize((256, 256), Image.LANCZOS).save(
        ico, format="ICO", sizes=[(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    )
    print("-> favicon.ico")


main()
