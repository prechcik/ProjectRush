using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    public string mobName;
    public int damage;
    public int maxHealth;
    public int currentHealth;
    public float attackDelay;
    public bool attacking;
    public bool awake;
    public float attackRange;
    public GameObject target;
    public Animator anim;

    public Text healthText;
    public Image healthBar;
    public Transform hpBarParent;
    public RectTransform hpBackground;

    public bool inCombat = false;
    public float combatTimer = 0f;
    

    [Range(0,24f)]
    public float time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float hpPercent = maxHealth / currentHealth;
        hpBackground.transform.position = Camera.main.WorldToScreenPoint(hpBarParent.transform.position);
        healthBar.fillAmount = hpPercent;
        healthText.text = currentHealth + "/" + maxHealth;
        
        if (time >= 6f && time <= 18f)
        {
            awake = false;
        } else
        {
            awake = true;
        }
        if (currentHealth < 1)
        {
            awake = false;
        }
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, attackRange);
        if (awake && playersInRange.Length > 0)
        {
            if (target == null)
            {
                foreach(Collider col in playersInRange)
                {
                    if (col.GetComponent<Player>() != null)
                    {
                        target = col.gameObject;
                        return;
                    } else
                    {
                        target = null;
                    }
                }
            }
        } else
        {
            target = null;
            attacking = false;
        }
        if (target != null && !attacking)
        {
            if (Vector3.Distance(this.transform.position, target.transform.position) > attackRange)
            {
                target = null;
                attacking = false;
                return;
            }
            if (target.GetComponent<Player>().currentHealth < 1)
            {
                target = null;
                attacking = false;
                return;
            }
            this.transform.LookAt(target.transform);
            attacking = true;
            DoDamage(target.GetComponent<NetworkIdentity>(), damage);
            anim.SetTrigger("Attack");
            
            Invoke(nameof(ResetAttack), attackDelay);
        }
        combatTimer -= Time.deltaTime;
        if (combatTimer <= 0f)
        {
            inCombat = false;
        } else
        {
            inCombat = true;
        }
        if (inCombat)
        {
            hpBackground.gameObject.SetActive(true);
        } else
        {
            hpBackground.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(33f, 214f, 242f, 0.37f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }

    public void UpdateTime(float time)
    {
        this.time = time * 24f ;
    }

    
    public void DoDamage(NetworkIdentity player, int damage)
    {
        player.GetComponent<Player>().ReceiveDamage(damage);
        inCombat = true;
        ResetCombatTimer();
    }

    public void ReceiveDamage(int damage)
    {
        if (damage < currentHealth)
        {
            currentHealth -= damage;
            // Show damage text
        } else
        {
            // Died
            currentHealth = 0;
            awake = false;
            attacking = false;
            target = null;
        }
        
    }

    public void ResetAttack()
    {
        attacking = false;
    }

    public void ResetCombatTimer()
    {
        combatTimer = 10f;
    }
}
