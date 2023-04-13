using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        #region Variables
        [Header("Movement")]
        public float vitesseHorizontale;
        public float vitesseVerticale;
        public float vitesseDeplacement;
        float rotationH;
        float rotationV;
        float vDeplacement;
        float hDeplacement;
        [Header("Component")]
        Rigidbody rb;
        public Slider slider;
        public TMP_Text playerNameText;
        public GameObject floatingInfo;
        [SerializeField] private Camera camera;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject leftArmPoint;
        [SerializeField] private GameObject rightArmPoint;

        private Material playerMaterialClone;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;
        public Munition munitionScript;
        private SceneScript sceneScript;
        private float nextFire = 0.0f;
        private float nextFire2 = 0.0f;
        public GameObject weapon1;
        public GameObject weapon2;
        [SerializeField] private Image replaceImg;
        bool onPad;
        bool canSwapWeapon1;
        bool canSwapWeapon2;

        #endregion
        void Awake()
        {
            //allow all players to run this
            sceneScript = GameObject.Find("SceneReference").GetComponent<SceneReference>().sceneScript;

        }
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            rb = GetComponent<Rigidbody>();
            munitionScript.UIAmmo1(weapon1.GetComponent<Weapon>().weaponAmmo);
            munitionScript.UIAmmo2(weapon2.GetComponent<Weapon>().weaponAmmo);
        }
        ////send message to other players
        //[Command]
        //public void CmdSendPlayerMessage()
        //{
        //    if (sceneScript)
        //        sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        //}

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            playerMaterialClone = new Material(GetComponent<Renderer>().material);
            playerMaterialClone.color = _New;
            GetComponent<Renderer>().material = playerMaterialClone;
        }

        public override void OnStartLocalPlayer()
        {
            sceneScript.playerScript = this;

            camera.gameObject.SetActive(true);
            canvas.gameObject.SetActive(true);
            //place the camera to the good place
            //Camera.main.transform.SetParent(transform);
            //Camera.main.transform.localPosition = new Vector3(0, 1.6f, 0);

            //sets a "Random" name and color to the new player

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);

            // Find all children of this game object
            Transform[] childTransforms = GetComponentsInChildren<Transform>();

            // Disable mesh renderers on all children with the tag "ADesactiver"
            foreach (Transform childTransform in childTransforms)
            {
                GameObject childObject = childTransform.gameObject;
                if (childObject.CompareTag("DesactivateOnPlay"))
                {
                    MeshRenderer renderer = childObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            //sceneScript.statusText = $"{playerName} joined.";
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                // make non-local players run this
                return;
            }
            rotationH = Input.GetAxis("Mouse X") * vitesseHorizontale;
            transform.Rotate(0, rotationH, 0);

            rotationV += Input.GetAxis("Mouse Y") * vitesseVerticale;
            rotationV = Mathf.Clamp(rotationV, -45, 45);

            camera.transform.localEulerAngles = new Vector3(-rotationV, 0, 0);
            vDeplacement = Input.GetAxis("Vertical") * vitesseDeplacement;
            hDeplacement = Input.GetAxis("Horizontal") * vitesseDeplacement;
            GetComponent<Rigidbody>().velocity = transform.forward * vDeplacement + transform.right * hDeplacement + new Vector3(0, rb.velocity.y, 0);

            //handle firing
            WeaponsFiring();

            //handle weapon swaping
            if (Input.GetKeyDown(KeyCode.Alpha1) && onPad)
            {
                canSwapWeapon1 = true;
            }
            
        }
        #region weaponFire
        void WeaponsFiring()
        {
            if (Input.GetButton("Fire1")) //Fire gun Left
            {
                if (weapon1.GetComponent<Weapon>().weaponAmmo > 0 && Time.time > nextFire)
                {
                    nextFire = Time.time + weapon1.GetComponent<Weapon>().weaponCooldown;
                    weapon1.GetComponent<Weapon>().weaponAmmo -= 1;
                    munitionScript.UIAmmo1(weapon1.GetComponent<Weapon>().weaponAmmo);

                    
                    RaycastHit hitInfo;
                    if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo))
                    {
                        if (hitInfo.point != null)
                        {

                            //Give a direction 
                            Vector3 direction = (hitInfo.point - weapon1.GetComponent<Weapon>().weaponFirePosition.position).normalized;
                            CmdShootRay(1, direction);
                            Vector3 cameraRotation = camera.transform.localEulerAngles;
                            cameraRotation.y += 0.1f; // adjust the y-axis Rotation
                            camera.transform.localEulerAngles = cameraRotation;

                        }
                    }
                    else
                    {
                        CmdShootRayVoid(1);
                    }
                }
            }
            if (Input.GetButton("Fire2")) //Fire gun Right
            {
                if (weapon2.GetComponent<Weapon>().weaponAmmo > 0 && Time.time > nextFire2)
                {
                    nextFire2 = Time.time + weapon2.GetComponent<Weapon>().weaponCooldown;
                    weapon2.GetComponent<Weapon>().weaponAmmo -= 1;
                    munitionScript.UIAmmo2(weapon2.GetComponent<Weapon>().weaponAmmo);

                    RaycastHit hitInfo;
                    if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo))
                    {
                        if (hitInfo.point != null)
                        {
                            //Give a direction 
                            Vector3 direction = (hitInfo.point - weapon2.GetComponent<Weapon>().weaponFirePosition.position).normalized;
                            CmdShootRay(2, direction);
                        }
                    }
                    else
                    {
                        CmdShootRayVoid(2);
                    }
                }
            }
        }
        [Command]
        void CmdShootRay(int _weapon, Vector3 _direction)
        {
            if (_weapon == 1)
            {
                RpcFireWeapon(1,_direction);
            }
            if (_weapon == 2)
            {
                RpcFireWeapon(2,_direction);
            }

        }
        [Command]
        void CmdShootRayVoid(int _weapon)
        {
            if (_weapon == 1)
            {
                RpcFireWeaponVoid(1);
            }
            if (_weapon == 2)
            {
                RpcFireWeaponVoid(2);
            }

        }

        [ClientRpc]
        void RpcFireWeapon(int _weapon, Vector3 _direction)
        {
            if (_weapon == 1 && _direction != null)
            {
                GameObject bullet = Instantiate(weapon1.GetComponent<Weapon>().weaponBullet, weapon1.GetComponent<Weapon>().weaponFirePosition.position, weapon1.GetComponent<Weapon>().weaponFirePosition.rotation);
                bullet.GetComponent<Rigidbody>().velocity = _direction * weapon1.GetComponent<Weapon>().weaponSpeed;
                Destroy(bullet, weapon1.GetComponent<Weapon>().weaponLife);

            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (_weapon == 2 && _direction != null)
            {
                GameObject bullet = Instantiate(weapon2.GetComponent<Weapon>().weaponBullet, weapon2.GetComponent<Weapon>().weaponFirePosition.position, weapon2.GetComponent<Weapon>().weaponFirePosition.rotation);
                bullet.GetComponent<Rigidbody>().velocity = _direction * weapon2.GetComponent<Weapon>().weaponSpeed;
                Destroy(bullet, weapon2.GetComponent<Weapon>().weaponLife);
            }
        }
        [ClientRpc]
        void RpcFireWeaponVoid(int _weapon)
        {
            if(_weapon == 1)
            {
                GameObject bullet = Instantiate(weapon1.GetComponent<Weapon>().weaponBullet, weapon1.GetComponent<Weapon>().weaponFirePosition.position, weapon1.GetComponent<Weapon>().weaponFirePosition.rotation);
                bullet.GetComponent<Rigidbody>().velocity = camera.transform.forward * weapon1.GetComponent<Weapon>().weaponSpeed;
                Destroy(bullet, weapon1.GetComponent<Weapon>().weaponLife);
            }
            if (_weapon == 2)
            {
                GameObject bullet = Instantiate(weapon2.GetComponent<Weapon>().weaponBullet, weapon2.GetComponent<Weapon>().weaponFirePosition.position, weapon2.GetComponent<Weapon>().weaponFirePosition.rotation);
                bullet.GetComponent<Rigidbody>().velocity = camera.transform.forward * weapon2.GetComponent<Weapon>().weaponSpeed;
                Destroy(bullet, weapon2.GetComponent<Weapon>().weaponLife);
            }
        }
        #endregion weaponFire


        [Command(requiresAuthority = false)]
        void CmdLoseHealth(int _amount)
        {
            RpcLoseHealth(_amount);
        }
        [ClientRpc]
        void RpcLoseHealth(int _amount)
        {
           
            slider.value -= _amount;
        }
        [Command]
        void CmdChangeWeapon(GameObject weapon, string name)
        {
            RpcChangeWeapon(weapon, name);
        }
        [ClientRpc]
        void RpcChangeWeapon(GameObject weapon,string name)
        {

            //Destroy that object
            Destroy(weapon.transform.GetChild(0).gameObject);
            //Destroy leftArm
            Destroy(leftArmPoint.transform.GetChild(0).gameObject);

            //instantiate this object as a gameobject
            GameObject arm = Instantiate(Resources.Load(name), leftArmPoint.transform.position, leftArmPoint.transform.rotation) as GameObject;
            //make it child of a spawnpoint
            arm.transform.parent = leftArmPoint.gameObject.transform;
            //tag it
            arm.gameObject.tag = "WeaponLeft";
            //make the script know that it as changed
            weapon1 = arm;
            //update weapon ammo
            munitionScript.UIAmmo1(weapon1.GetComponent<Weapon>().weaponAmmo);


            //TODO
            //Add SFX VFX
            //Add animations / or add explosion force

        }
        void OnTriggerEnter(Collider other)
        {

            if (other.GetComponent<Collider>().gameObject.tag == "bulletBasic")
            {
                print("je touche une balle");
                CmdLoseHealth(5);
                Destroy(other);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<Collider>().gameObject.tag == "WeaponPad" && other.transform.childCount > 0)
            {
                print("show UI");
                replaceImg.enabled = true;
                onPad = true;
                if (canSwapWeapon1)
                {
                    canSwapWeapon1 = false;
                    GameObject weaponpad = other.gameObject;
                    //Get the name of the weapon on pad
                    name = weaponpad.transform.GetChild(0).gameObject.name;
                    Invoke("HideUI", 0.1f);
                    CmdChangeWeapon(weaponpad, name);
                    
                }
                if (canSwapWeapon2)
                {
                    print("I change weapon2");
                    Destroy(other.transform.GetChild(0));
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {

            if (other.GetComponent<Collider>().gameObject.tag == "WeaponPad")
            {
                onPad = false;
                print("Don't show UI");
                replaceImg.enabled = false;
            }
        }
        void HideUI()
        {
            //hide img replace weapon
            replaceImg.enabled = false;
        }
    }//end class
}//end namespace