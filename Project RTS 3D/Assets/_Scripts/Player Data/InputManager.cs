using System;
using UnityEngine;

//Responsible for controlling the player's camera.
public class InputManager : MonoBehaviour
{
    [SerializeField] private float panSpeed;
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private Vector2 panLimit;
    [SerializeField] private Vector2 scrollLimit;

    public static Action<RaycastHit> OnRightClick;

    private Camera mainCamera;

    #region Terrain
    private Terrain terrain;
    private float oldAltitude; 
    private float altitude;
    #endregion

    private void Start()
    {
        mainCamera = Camera.main;
        terrain = Terrain.activeTerrain;
    }

    void Update()
    {
        MoveCamera();
        if (Input.GetMouseButtonDown(1))
        {
            RightClick();
        }
    }

    private void RightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            OnRightClick?.Invoke(hit);
        }
        
    }

    /*A formal method made to make the camera move in the game when the cursor is on the outskirts, 
     * without annoying in the editor.
     */
    private bool OnEditorCondition(int i)
    {
    #if UNITY_EDITOR
        return false;
#else
        switch (i)
        {
            case 1:
                return Input.mousePosition.y >= Screen.height - panLimit.y;
            case 2:
                return Input.mousePosition.y <= panLimit.y;
            case 3:
                return Input.mousePosition.x >= Screen.width - panLimit.x;
            case 4:
                return Input.mousePosition.x <= panLimit.x;
        }
        return true;
#endif
    }

    //Camera movement
    void MoveCamera()
    {
        Vector3 pos = transform.position;

        //It will be necessary to replace WASD with arrow control later
        if (Input.GetKey(KeyCode.W) || OnEditorCondition(1))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || OnEditorCondition(2))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || OnEditorCondition(3))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || OnEditorCondition(4))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        TerrainChange();

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, scrollLimit.x + altitude, scrollLimit.y + altitude);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);
        transform.position = pos;
    }

    //Changing the camera depending on the Terrain
    //Needs to be improved in the future
    void TerrainChange()
    {
        if (terrain != null)
        {
            altitude = terrain.SampleHeight(transform.position);
            if (oldAltitude != altitude)
            {
                Vector3 newPos = transform.position;
                newPos.y = altitude;
                newPos.y += transform.position.y;
                transform.position = newPos;
                oldAltitude = altitude;
            }
        }
    }

}