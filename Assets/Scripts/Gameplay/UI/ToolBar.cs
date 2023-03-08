using UnityEngine;

public class ToolBar : MonoBehaviour
{
    [SerializeField]
    public GameObject bagInspector;
    // Start is called before the first frame update
    void Start()
    {
        bagInspector.SetActive(false);
    }

    public void OnClickBag()
    {
        bagInspector.SetActive(true);
    }
}
