using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDetector : MonoBehaviour
{
    public Grid targetGrid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float detectionZDepth = -1f;
    [SerializeField] private GameObject mark;
    [SerializeField] float markLifetime = 0.1f;
    private readonly Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    WaitForSeconds markWait;

    void Awake()
    {
        markWait = new WaitForSeconds(markLifetime);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager.instance.GetState())
        {
            OnClick();
        }
    }

    void OnClick()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z + detectionZDepth);
        Vector3 worldClickPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector3 clickedPos = targetGrid.GetCellCenterWorld(targetGrid.WorldToCell(new Vector3(worldClickPosition.x, worldClickPosition.y, detectionZDepth)));
        ProcessTileMatch(clickedPos);
    }

    void ProcessTileMatch(Vector3 clickedPos)
    {
        Vector2 origin = new(clickedPos.x, clickedPos.y); // 레이캐스트 원점
        if (Physics2D.OverlapPoint(origin) != null)
        {
            // Debug.Log("클릭한 칸이 비어있지 않습니다.");
            return;
        }

        List<GameObject> tiles = FindTiles(origin);
        if (tiles.Count < 2)
        {
            // Debug.Log("타일이 하나 이하입니다. 매치되는 타일이 없습니다.");
            GameManager.instance.SetTime(-10f);
            return;
        }

        Dictionary<string, List<GameObject>> groups = GroupTiles(tiles);
        List<GameObject> tilesToDestroy = CountMatchedTiles(groups, clickedPos);
        if (tilesToDestroy.Count == 0)
        {
            // Debug.Log("매치되는 타일이 존재하지 않습니다.");
            GameManager.instance.SetTime(-10f);
            return;
        }
        else
        {
            RemoveTiles(tilesToDestroy);
            GameManager.instance.AddScore(tilesToDestroy.Count);
        }
    }

    List<GameObject> FindTiles(Vector2 origin) // 타일 탐색
    {
        List<GameObject> tiles = new();

        foreach (Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, dir);
            if (hit.collider != null)
            {
                tiles.Add(hit.collider.gameObject);
            }
        }


        return tiles;
    }

    Dictionary<string, List<GameObject>> GroupTiles(List<GameObject> tiles) // 타일 그룹화
    {
        Dictionary<string, List<GameObject>> groups = new();
        foreach (GameObject tile in tiles)
        {
            if (!groups.ContainsKey(tile.tag))
                groups[tile.tag] = new List<GameObject>();
            groups[tile.tag].Add(tile);
        }
        return groups;
    }

    void RemoveTiles(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<Collider2D>().enabled = false;
            Destroy(tile, markLifetime);
        }
    }

    List<GameObject> CountMatchedTiles(Dictionary<string, List<GameObject>> groups, Vector3 clickedPos)
    {
        List<GameObject> objsToDestroy = new();
        foreach (var pair in groups)
        {
            if (pair.Value.Count >= 2) // 같은 종류의 타일이 2개 이상 발견되면
            {
                foreach (GameObject obj in pair.Value)
                {
                    Vector3 objPos = targetGrid.GetCellCenterWorld(targetGrid.WorldToCell(obj.transform.position));

                    StartCoroutine(MakeMarkGridPath(clickedPos, objPos));
                    objsToDestroy.Add(obj); // 파괴할 오브젝트 리스트에 추가
                }
            }
        }
        return objsToDestroy;
    }

    IEnumerator MakeMarkGridPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector3Int startCell = targetGrid.WorldToCell(startWorldPos);
        Vector3Int endCell = targetGrid.WorldToCell(endWorldPos);

        Vector3Int currentCell = startCell;

        bool isHorizontal = startCell.y == endCell.y;

        while (currentCell != endCell)
        {
            // 마크 생성
            Vector3 cellWorldPos = targetGrid.GetCellCenterWorld(currentCell);
            cellWorldPos.z = detectionZDepth;
            GameObject m = Instantiate(mark, cellWorldPos, Quaternion.identity, transform);
            Destroy(m, markLifetime);

            // 다음 셀로 이동
            if (isHorizontal)
                currentCell.x += (endCell.x > currentCell.x) ? 1 : -1;
            else
                currentCell.y += (endCell.y > currentCell.y) ? 1 : -1;
        }

        // 마크가 사라지는 시간 기다리기
        yield return markWait;
    }

}