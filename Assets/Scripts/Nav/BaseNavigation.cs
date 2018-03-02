using System.Collections;
using UnityEngine.AI;
using UnityEngine;

namespace Nav {

	public class BaseNavigation : MonoBehaviour {

        //寻路目标
        [SerializeField]Transform m_target;

        //寻路组件
        NavMeshAgent m_agent;

        void Start () {
            m_agent = GetComponent<NavMeshAgent>();
        }
		
		void Update () {
            //设置寻路
            if(m_agent && m_target) {
                m_agent.SetDestination(m_target.position);
            }
        }
	}
}