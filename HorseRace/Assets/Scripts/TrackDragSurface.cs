using UnityEngine;
using UnityEngine.EventSystems;

namespace HorseRace
{
    public sealed class TrackDragSurface : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private HorseRaceGame owner;

        public void Initialize(HorseRaceGame game)
        {
            owner = game;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            owner?.BeginTrackDrag();
        }

        public void OnDrag(PointerEventData eventData)
        {
            owner?.DragTrack(eventData.delta.x);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            owner?.EndTrackDrag();
        }
    }
}
