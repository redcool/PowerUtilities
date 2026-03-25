using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public interface IMono
    {
        public void Awake();
        public void Start();
        public void Update();

        public void OnEnable();
        public void OnDisable();
        public void OnDestroy();

    }

    public class BaseMono : IMono
    {
        public void Awake()
        {
        }

        public void OnDestroy()
        {
        }

        public void OnDisable()
        {
        }

        public void OnEnable()
        {
        }

        public void Start()
        {
        }

        public void Update()
        {
        }
    }
}
