using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace View {
    public class PriorityPanelScript : MonoBehaviour
    {
        // Start is called before the first frame update
        public OrchestratorViewHandler handler;
        public GameObject[] iconSelections;
        public GameObject p1;
        public GameObject p2;
        public GameObject selectionMarker;
        public int? highlightedIndex;
        public int chosenIndex;


        public void handlePrioritySelection()
        {
            chosenIndex = (int) highlightedIndex;
            Debug.Log(chosenIndex);
            float selectionHeight = iconSelections[chosenIndex].GetComponent<RectTransform>().rect.height;
            float selectionWidth = iconSelections[chosenIndex].GetComponent<RectTransform>().rect.height;
            p1.transform.position = iconSelections[chosenIndex].transform.position + new Vector3(-selectionWidth/4, -selectionHeight*3/4, 0);
            p2.transform.position = iconSelections[chosenIndex].transform.position + new Vector3(selectionWidth / 4, -selectionHeight * 3 / 4, 0);
            p1.SetActive(true);
            p2.SetActive(true);
        }


        private void resetPanel()
        {
            p1.SetActive(false);
            p2.SetActive(false);
            selectionMarker.SetActive(false);
        }

        public void handlePriorityInput(int value)
        {
            resetPanel();
            handler.handlePriorityInput(chosenIndex,value);
        }

        public void highlightIcon(int index)
        {
            highlightedIndex = index;
            selectionMarker.SetActive(true);
            Debug.Log(index);
            selectionMarker.transform.position = iconSelections[index].transform.position;
        }

        public void mouseOut()
        {

            highlightedIndex = null;
            selectionMarker.SetActive(false);
        }

        public void mouseIn(int index)
        {
            if (highlightedIndex != null) return;
            highlightIcon(index);
        }

     
    }

}
