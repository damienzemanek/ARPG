using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EMILtools.Extensions
{
    [Serializable]
    public struct FadeSettings
    {
        public float step;
        public float delay;
        public float delayToStartFading;

        public GameObject GetGO(Object targ)
        {
            switch (targ)
            {
                case GameObject go: return go;
                case UnityEngine.Component c: return c.gameObject; 
                default: return null;
            }
        }

        public void SetColor(Color c, Object targ)
        {
            switch (targ)
            {
                case Graphic g:
                    g.color = c;
                    break;
                case Material m:
                    m.color = c;
                    break;
                case Renderer r:
                    r.material.color = c;
                    break;
                case GameObject go:
                    if (go.TryGetComponent<Graphic>(out var gg))
                        gg.color = c;
                    else if (go.TryGetComponent<Renderer>(out var rr))
                        rr.material.color = c;
                    break;
            }
        }

        public Color GetColor(Object targ)
        {
            switch (targ)
            {
                case Graphic g: return g.color;
                case Material m: return m.color;
                case Renderer r: return r.material.color;
                case GameObject go:
                    if (go.TryGetComponent<Graphic>(out var gg))
                        return gg.color;
                    if (go.TryGetComponent<Renderer>(out var rr))
                        return rr.material.color;
                    break;
            }
            return Color.red;
        }

        public void SetAlpha(float val, Object targ)
        {
            switch(targ)
            {
                case Graphic g:
                    Color gc = g.color;
                    gc.a = val;
                    g.color = gc;
                    break;
                case Material m:
                    Color mc = m.color;
                    mc.a = val;
                    m.color = mc;
                    break;
                case Renderer r:
                    Color rc = r.material.color;
                    rc.a = val;
                    r.material.color = rc;
                    break;
            }
        }
    }
}
