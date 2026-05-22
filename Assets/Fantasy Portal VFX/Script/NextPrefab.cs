using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NextPrefab
{
    public class NextPrefab : MonoBehaviour
    {
        #region Variables

        // Elements
        public GameObject[] m_PrefabList;

        // Index of current element
        int m_CurrentElementIndex = -1;

        // Index of current particle
        int m_CurrentParticleIndex = -1;

        // Current particle list
        GameObject[] m_CurrentElementList = null;

        // GameObject of current particle that is showing in the scene
        GameObject m_CurrentParticle = null;

        public Text m_ParticleName;

        #endregion

        #region MonoBehaviour Functions

        // Use this for initialization
        void Start()
        {
            // Check if there is any particle in prefab list
            if (m_PrefabList.Length > 0)
            {
                // Reset indices of element and particle
                m_CurrentElementIndex = 0;
                m_CurrentParticleIndex = 0;

                // Show particle
                ShowParticle();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Check if there is any particle in prefab list
            if (m_CurrentElementIndex != -1 && m_CurrentParticleIndex != -1)
            {
                // User released Left arrow key
                if (Input.GetKeyUp(KeyCode.Z))
                {
                    m_CurrentParticleIndex--;
                    ShowParticle();
                }
                // User released Right arrow key
                else if (Input.GetKeyUp(KeyCode.X))
                {
                    m_CurrentParticleIndex++;
                    ShowParticle();
                }
                
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    RespawnParticle();
                }
            }
        }

        #endregion

        #region Functions
        void RespawnParticle()
        {
            // Destroy the current particle if it exists
            if (m_CurrentParticle != null)
            {
                Destroy(m_CurrentParticle);
            }

            // Instantiate a new particle
            m_CurrentParticle = (GameObject)Instantiate(m_CurrentElementList[m_CurrentParticleIndex]);
        }
        // Remove old Particle and create new Particle GameObject
        void ShowParticle()
        {
            // Update current m_CurrentElementList and m_ElementName
            if (m_CurrentElementIndex == 0)
            {
                m_CurrentElementList = m_PrefabList;
            }

            // Make m_CurrentParticleIndex be rounded
            if (m_CurrentParticleIndex >= m_CurrentElementList.Length)
            {
                m_CurrentParticleIndex = 0;
            }
            else if (m_CurrentParticleIndex < 0)
            {
                m_CurrentParticleIndex = m_CurrentElementList.Length - 1;
            }

            // Update current m_ParticleName
            m_ParticleName.text = m_CurrentElementList[m_CurrentParticleIndex].name;

            // Remove old particle
            if (m_CurrentParticle != null)
            {
                Destroy(m_CurrentParticle);
            }

            // Create new particle
            m_CurrentParticle = (GameObject)Instantiate(m_CurrentElementList[m_CurrentParticleIndex]);
        }

        public void OnLeftArrowPressed()
        {
            m_CurrentParticleIndex--;
            ShowParticle();
        }

        // Handles right arrow key or button press
        public void OnRightArrowPressed()
        {
            m_CurrentParticleIndex++;
            ShowParticle();
        }

        #endregion
    }
}
