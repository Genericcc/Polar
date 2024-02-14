using _Scripts.Grid;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts
{
    public class Unit : MonoBehaviour
    {

        [SerializeField] private Animator unitAnimator;


        private Vector3 targetPosition;
        private PolarGridPosition polarGridPosition;

        private void Awake()
        {
            targetPosition = transform.position;
        }

        private void Start()
        {
            //polarGridPosition = GridManager.Instance.GetGridPosition(transform.position);
            //GridManager.Instance.AddUnitAtGridPosition(polarGridPosition, this);
        }

        private void Update()
        {
            var moveSpeed = 4f;
            var stoppingDistance = .1f;
            var rotateSpeed = 10f;
            
            if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
            {
                var moveDirection = (targetPosition - transform.position).normalized;
                transform.position += moveDirection * (moveSpeed * Time.deltaTime);

                transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

                unitAnimator.SetBool("IsWalking", true);
            } else
            {
                unitAnimator.SetBool("IsWalking", false);
            }


            var newPolarGridPosition = new PolarGridPosition(0, 0, 0, 0);//GridManager.Instance.GetGridPosition(transform.position);
            if (newPolarGridPosition != polarGridPosition)
            {
                // Unit changed Grid Position
                //GridManager.Instance.UnitMovedGridPosition(this, polarGridPosition, newPolarGridPosition);
                polarGridPosition = newPolarGridPosition;
            }
        }

        public void Move(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;
        }
    }
}