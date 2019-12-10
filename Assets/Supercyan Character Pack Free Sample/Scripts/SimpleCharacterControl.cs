using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Playables;
using System.Collections;

public class SimpleCharacterControl : MonoBehaviour {

    private enum ControlMode
    {
        Tank,
        Direct
    }

    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_turnSpeed = 200;
    [SerializeField] private float m_jumpForce = 4;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;

    public TMP_Text instructions;

    [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 0.33f;
    private readonly float m_backwardsWalkScale = 0.16f;
    private readonly float m_backwardRunScale = 0.66f;

    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;
    private int gem_count = 1;

    private bool m_isGrounded;
    private float old_distance = 1000f;
    private List<Collider> m_collisions = new List<Collider>();
    private bool jumpIn = false;
    public PlayableDirector cutscene;

    public GameObject ruby2;
    public GameObject ruby3;
    public GameObject ruby4;
    public GameObject ruby5;

    public void Start() {
      ruby2.SetActive(false);
      ruby3.SetActive(false);
      ruby4.SetActive(false);
      ruby5.SetActive(false);

    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for(int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ruby"))
        {
            other.gameObject.SetActive (false);
            if (gem_count == 1) {
              instructions.text = "You found your first gem! Press 'R' to help you find the next one.";
              ruby2.SetActive(true);
            } else if (gem_count == 2) {
              instructions.text = "You feel motivated! You run twice as fast!";
              m_moveSpeed = m_moveSpeed * 2;
              ruby3.SetActive(true);
            } else if (gem_count == 3) {
              instructions.text = "You found your third gem! Speed bump!!";
              m_moveSpeed += 2;
              ruby4.SetActive(true);
            } else if (gem_count == 4) {
              instructions.text = "Only one more! Press 'R' to help you find the last one.";
              ruby5.SetActive(true);
            } else {
              instructions.text = "You feel the volcano pulling you in. Do you want to jump in? (y/n)";
              m_moveSpeed = 0;
              m_turnSpeed = 0;
              jumpIn = true;
            }
            old_distance = 5000;
            gem_count++;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if(validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        } else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }

    void Update()
    {
        m_animator.SetBool("Grounded", m_isGrounded);

        if (Input.GetKeyDown("r"))
        {
          float distance = 0;
          string s = "You rub your gem. ";
          if (gem_count == 2) {
            distance = Vector3.Distance(transform.position, ruby2.transform.position);
          } else if (gem_count == 3) {
            distance = Vector3.Distance(transform.position, ruby3.transform.position);
          } else if (gem_count == 4) {
            distance = Vector3.Distance(transform.position, ruby4.transform.position);
          } else if (gem_count == 5) {
            distance = Vector3.Distance(transform.position, ruby5.transform.position);
          }
          // [26, inf] very cold
          // [18, 26] cold
          // [12, 18] warm
          // [6, 12] hot
          // [2, 6] very hot
          // [0, 2] it practically burns your hand

          if (distance >= 26) {
            s += "It feels very cold";
          } else if (distance >= 18) {
            s += "It feels cool";
          } else if (distance >= 12) {
            s += "It feels warm";
          } else if (distance >= 6) {
            s += "It feels hot";
          } else if (distance >= 2) {
            s += "It feels very hot";
          } else {
            s += "It practically burns your hands!";
          }
          if (distance >= 2) {
            if (distance >= 18) {
              if (distance > old_distance) {
                s += ", and colder than before.";
              } else {
                s += ", but warmer than before.";
              }
            } else {
              if (distance > old_distance) {
                s += ", but colder than before.";
              } else {
                s += ", and warmer than before.";
              }
            }
          }
          old_distance = distance;
          instructions.text = s;
        }

        switch(m_controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        if (jumpIn)
        {
            if (Input.GetKeyDown("n"))
            {
                instructions.text = "You try to resist, but your body forces you in!";
            }
            if (Input.GetKeyDown("y"))
            {
                instructions.text = "You have the urge for death! You jump in.";
            }

            if (Input.GetKeyDown("y") || Input.GetKeyDown("n"))
            {
                double duration = cutscene.duration;
                cutscene.Play();
                StartCoroutine(Wait((float) duration));
            }
        }

        m_wasGrounded = m_isGrounded;
    }

    private void TankUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        bool walk = Input.GetKey(KeyCode.LeftShift);

        if (v < 0) {
            if (walk) { v *= m_backwardsWalkScale; }
            else { v *= m_backwardRunScale; }
        } else if(walk)
        {
            v *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
        transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

        m_animator.SetFloat("MoveSpeed", m_currentV);

        JumpingAndLanding();
    }

    private void DirectUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if(direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }

    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("Level1b");
    }
}
