using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class PlatformerController : MonoBehaviour
{

    public Spline attachedSpline;
    public float percentage = 0f;
    private float percentageLast = 0f;

    public float speed = 10f;

    //private Rigidbody rb;
    private CharacterController cc;

    private AnchorData anchorCurrent;
    private AnchorData anchorNext;
    private int anchorNextDirectionOnPath = 1;

    private List<AnchorData> anchorsOnSpline = new List<AnchorData>();

    private Vector2 inputDirections = Vector2.zero;
    private Vector2 velocity2D = Vector2.zero;
    private Vector3 direction = Vector3.zero;
    private Vector3 finalVelocity = Vector3.zero;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        
        // Add all anchors from this spline to the list containing the requested anchor data.
        for (int i = 0; i < attachedSpline.Anchors.Length; i++)
        {
            AnchorData anchorData = new AnchorData(attachedSpline.Anchors[i], i);
            anchorData.visited = false;
            anchorsOnSpline.Add(anchorData);
        }

        anchorCurrent = GetClosestAnchor(transform.position);
        SetAnchorsVisited(anchorCurrent.index);
        anchorNext = GetNextAnchor();

        TeleportCharacter(new Vector3(anchorCurrent.positionFlat.x, transform.position.y, anchorCurrent.positionFlat.z));

    }

    private void Update()
    {
        Vector3 myPos = transform.position;
        myPos.y = 0;

        GetInputs();
        CalculateVelocity2D();

        if (velocity2D.x != 0)
        {
            int velocitySignX = (int)Mathf.Sign(velocity2D.x);
            if (velocitySignX != anchorNextDirectionOnPath)
            {
                if (velocitySignX > 0)
                    anchorNext = GetNextAnchor();
                else
                    anchorNext = GetPreviousAnchor();
            }
            anchorNextDirectionOnPath = velocitySignX;

            AnchorData closestAnchor = GetClosestAnchor(myPos);

            if (anchorNext.anchor != null)
                Debug.DrawRay(anchorNext.positionFlat, Vector3.up * 2, Color.green);

            if (Vector3.Distance(myPos, closestAnchor.positionFlat) <= 0.0000001f || anchorNext.anchor == null)
            {
                anchorCurrent = closestAnchor;

                int splineDirectionModifier = (attachedSpline.direction == SplineDirection.Forward ? 1 : -1);

                int anchorNextID = anchorCurrent.index + velocitySignX * splineDirectionModifier;
                if (!attachedSpline.loop)
                    anchorNextID = Mathf.Clamp(anchorNextID, 0, attachedSpline.Anchors.Length - 1);
                anchorNextID = (int)Mathf.Repeat(anchorNextID, attachedSpline.Anchors.Length);

            }

            Vector3 directionToNextPoint = (anchorNext.positionFlat - myPos);
            directionToNextPoint.y = 0;

            // Draw debug line to next point.
            Debug.DrawLine(myPos, myPos + directionToNextPoint, Color.green);

            directionToNextPoint.Normalize();
            int oppositeModifier = anchorNextDirectionOnPath == velocitySignX ? 1 : -1;
            direction = directionToNextPoint;
            Vector3 moveAmount = (directionToNextPoint * speed * Time.deltaTime) * Mathf.Abs(velocity2D.x) * oppositeModifier;

            if (Vector3.Distance(myPos, anchorNext.positionFlat) <= moveAmount.magnitude)
            {
                moveAmount = (anchorNext.positionFlat - myPos);
                finalVelocity.x = moveAmount.x;
                finalVelocity.z = moveAmount.z;

                // Set anchor visited state to the appropriate state based on direction.
                anchorNext.visited = (velocitySignX > 0);

                if (anchorNext.visited)
                    anchorNext = GetNextAnchor();
                else
                    anchorNext = GetPreviousAnchor();
            }
        }

        Move();

        // IDEA!!!!!!!!!!!!! Divide player SPEED by the Spline LENGTH to get the "normalized" relative speed.
        // (Update: It worked!)

        // How to get player switching paths to the right position:
        // Raycast straight down from the center of the player. If you hit ground, change path and percentage to the closest point on the path to the raycast HIT position.
        // Potential problem: How do we determine when to switch paths, and when to switch paths, and when you're simply walking or flying above a path you're already on?
        // Potential solution: Store the current path. Always racast down. If the path hit is different than the current path, switch paths.
        // Potentail problem with that potential solution: How do you know you've hit a different path? The paths don't have colliders.
        // Potential solution (maybe): Have a script that the ground always has, and it stores which path the ground piece belongs to. Then, when you hit it with a raycast, it will take the path from that script.

        // NEW IDEA: 
        // Only use straight splines (no curves). 
        // Rather than using percentages and points on the path, just use the "anchors" and find the direction towards the next or previous anchor.
        // If distance to next anchor is less than the normalized direction multiplied by our speed, subtract the distance to the anchor from our speed, and move to the anchor immediately.
        // If there is no next or previous anchor, move in the inverse direction from the current anchor to the opposite direction anchor.

    }

    private void CalculateVelocity2D()
    {
        // X velocity
        if (inputDirections.x != 0f)
        {
            velocity2D.x = Mathf.Sign(inputDirections.x) * speed;
        }
        else
        {
            velocity2D.x = Mathf.Lerp(velocity2D.x, 0, 30 * Time.deltaTime);
        }

        // Y velocity

    }

    private void GetInputs()
    {
        inputDirections.x = Input.GetAxis("Horizontal");
    }

    private void Move()
    {

        Vector3 finalVelocity = Vector3.zero;
        int oppositeModifier = 1;
        if (Mathf.Sign(velocity2D.x) < 0) oppositeModifier = -1;
        finalVelocity = direction * (velocity2D.x * oppositeModifier);
        finalVelocity.y = velocity2D.y;

        cc.Move(finalVelocity * Time.deltaTime);

    }


    private void TeleportCharacter(Vector3 targetPosition)
    {
        cc.enabled = false;
        transform.position = targetPosition;
        cc.enabled = true;

    }


    public AnchorData GetClosestAnchor(Vector3 position)
    {
        AnchorData closestAnchor = null;
        float closestDistance = float.MaxValue;
        for( int i = 0; i < anchorsOnSpline.Count; i++)
        {
            AnchorData thisAnchor = anchorsOnSpline[i];

            float thisDistance = Vector3.Distance(position, thisAnchor.positionFlat);
            
            if (thisDistance < closestDistance) { 
                closestAnchor = thisAnchor;
                closestDistance = thisDistance;
                continue;
            }

        }

        if (closestAnchor != null)
        {

            return closestAnchor;
        }

        return null;
    }


    //public AnchorPair GetClosestAnchor(float percentOnPath)
    //{
    //    AnchorPair anchorPair = new AnchorPair();
    //    float closestDistance = float.MaxValue;
    //    float percentPerAnchor = 1 / (attachedSpline.Anchors.Length - 1);
    //    for (int i = 0; i < attachedSpline.Anchors.Length; i++)
    //    {
    //        float percent = percentPerAnchor * i;
    //        if (percent < percentOnPath) continue;

    //        SplineAnchor thisAnchor = attachedSpline.Anchors[i];
    //        float anchorPercent = attachedSpline.ClosestPoint(thisAnchor.Anchor.position, attachedSpline.Anchors.Length);

    //        float thisDistance = Mathf.Abs(anchorPercent - percentOnPath);

    //        Debug.DrawRay(attachedSpline.GetPosition(anchorPercent, true), Vector3.up, Color.blue);

    //        if (thisDistance < closestDistance)
    //        {
    //            anchorPair.anchor = thisAnchor;
    //            anchorPair.index = i;
    //            closestDistance = thisDistance;
    //            continue;
    //        }

    //    }

    //    if (anchorPair.anchor != null && anchorPair.index != -1)
    //    {
    //        return anchorPair;
    //    }
    //    return null;
    //}


    private AnchorData GetFirstAnchorInDirection(float percentOnPath, bool higher = true)
    {
        AnchorData anchorPair = new AnchorData();
        float percentPerAnchor = 1f / (attachedSpline.Anchors.Length - 1f);
        for (int i = 0; i < attachedSpline.Anchors.Length; i++)
        {
            float percent = percentPerAnchor * i;
            if (percent < percentOnPath) continue;

            SplineAnchor thisAnchor = attachedSpline.Anchors[i];

            anchorPair.anchor = thisAnchor;
            anchorPair.index = i;
            break;
        }

        if (anchorPair.anchor != null && anchorPair.index != -1)
        {
            return anchorPair;
        }
        return null;
    }


    private AnchorData GetNextAnchor()
    {
        foreach(AnchorData data in anchorsOnSpline)
        {
            if (data.visited) continue;
            return data;
        }

        if (attachedSpline.loop)
        {
            foreach (AnchorData data in anchorsOnSpline) data.visited = false;
            return anchorsOnSpline[0];
        }

        return anchorNext;
    }


    private AnchorData GetPreviousAnchor()
    {
        for (int i = anchorsOnSpline.Count - 1; i >= 0; i--)
        {
            AnchorData data = anchorsOnSpline[i];
            if (!data.visited) continue;
            return data;
        }

        if (attachedSpline.loop)
        {
            foreach (AnchorData data in anchorsOnSpline) data.visited = true;
            return anchorsOnSpline[anchorsOnSpline.Count - 1];
        }

        return anchorNext;
    }


    private void SetAnchorsVisited(int max)
    {
        foreach(AnchorData anchorData in anchorsOnSpline)
        {
            if (anchorData.index <= max) anchorData.visited = true;
            else anchorData.visited = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        MoveToOtherSplinePath moveToOtherSplinePath = other.GetComponent<MoveToOtherSplinePath>();
        if (moveToOtherSplinePath)
        {
            attachedSpline = moveToOtherSplinePath.targetSpline;
        }
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        percentage = percentageLast;
    }


    private void OnDrawGizmos()
    {
        // Draw closest anchor position sphere.
        if (anchorCurrent != null)
        {
            Gizmos.color = Color.red;
            if (anchorCurrent.anchor != null)
            Gizmos.DrawSphere(anchorCurrent.anchor.transform.position, 0.25f);
            Gizmos.color = Color.green;
            if (anchorNext.anchor != null)
                Gizmos.DrawSphere(anchorNext.anchor.transform.position, 0.25f);
        }
    }


    public class AnchorData
    {
        public SplineAnchor anchor = null;
        public int index;
        public bool visited = false;

        public Vector3 positionFlat
        {
            get { return new Vector3(anchor.transform.position.x, 0, anchor.transform.position.z); }
        }

        public AnchorData() { }

        public AnchorData(SplineAnchor _anchor, int _index)
        {
            anchor = _anchor;
            index = _index;
        }
    }


}
