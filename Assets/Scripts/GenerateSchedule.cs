using System;
using System.Linq;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum CourseRelationshipType
{
    Prerequisite,
    Corequisite,
    Either
}


public class Course
{
    public string CourseID { get; set; }
    public int CreditHours { get; set; }
    //public string Title { get; set; }
    public List<Course> Prerequisites { get; set; }
    public List<Course> Corequisites { get; set; }
}

public class Major
{
    public string MajorID { get; set; }
    public string Field { get; set; }
    //public string Title { get; set; }
    public List<Course> Requisites { get; set; }
}

public class Minor
{
    public string MinorID { get; set; }
    public string Field { get; set; }
    //public string Title { get; set; }
    public List<Course> Requisites { get; set; }
}

public class GenerateSchedule : MonoBehaviour
{
    GameObject VLContent;
    public GameObject HLContent;
    public GameObject baseText;
    public GameObject baseButton;
    List<GameObject> VLContentChildren;
    List<GameObject> HLContentChildren;

    List<Course> courses;
    Dictionary<string, Course> courseDict;
    List<Major> majors;
    Dictionary<string, Major> majorDict;
    List<Minor> minors;
    Dictionary<string, Minor> minorDict;
    Dictionary<Course, List<Course>> graph;


    // Start is called before the first frame update
    void UpdateUI()
    {
        foreach (GameObject obj in VLContentChildren)
        {
            Destroy(obj);
        }
        VLContentChildren.Clear();
        HLContentChildren.Clear();

        Major testMajor = majorDict["Journalism, Media Studies Concentration, B.S."];

        List<Course> temp = testMajor.Requisites;

        int semester=0;
        int semesterCredits = 0;
        int year = 2022;

        VLContent = new GameObject();
        VLContent.transform.SetParent(HLContent.transform);
        VerticalLayoutGroup vlg = VLContent.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;

        while (temp.Count>0)
        {
            string sName;
            if (semester == 0)
            {
                sName = "Spring";
            }
            else if (semester == 1)
            {
                sName = "Summer";
            }
            else
            {
                sName = "Fall";
            }
            GameObject textGO = Instantiate(baseText);
            textGO.transform.SetParent(VLContent.transform);
            Text myText = textGO.GetComponent<Text>();
            myText.text = sName + " " + year;
            while (semesterCredits<18&& temp.Count > 0)
            {
                Course tempCourse = temp[0];
                if(semesterCredits+tempCourse.CreditHours>18)
                {
                    break;
                }
                temp.RemoveAt(0);
                semesterCredits += tempCourse.CreditHours;

                GameObject buttonGO = Instantiate(baseButton);
                buttonGO.transform.SetParent(VLContent.transform);
                Button button = buttonGO.GetComponent<Button>();

                GameObject btextGO = buttonGO.transform.GetChild(0).gameObject;
                Text bText = btextGO.GetComponent<Text>();
                bText.text = tempCourse.CourseID;
            }
            semesterCredits = 0;
            if (semester == 2)
            {
                VLContent = new GameObject();
                VLContent.transform.SetParent(HLContent.transform);
                vlg = VLContent.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = false;
                vlg.childControlWidth = false;
                semester = 0;
                year++;
            }
            else semester++;
        }
    }
    void Start()
    {
        // Load the XML file
        VLContentChildren = new List<GameObject>();
        HLContentChildren = new List<GameObject>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load("Assets/mtsuwebdata - Copy.xml");

        // Extract course information
        XmlNodeList courseNodes = xmlDoc.SelectNodes("//course");
        XmlNodeList majorNodes = xmlDoc.SelectNodes("//major");
        XmlNodeList minorNodes = xmlDoc.SelectNodes("//minor");
        courses = new List<Course>();
        courseDict = new Dictionary<string, Course>();
        majors = new List<Major>();
        majorDict = new Dictionary<string, Major>();
        minors = new List<Minor>();
        minorDict = new Dictionary<string, Minor>();
        foreach (XmlNode courseNode in courseNodes)
        {
            if (courseNode.Attributes["courseID"] != null && courseNode.Attributes["creditHours"] != null)
            {
                Course course = new Course();
                course.CourseID = courseNode.Attributes["courseID"].Value;
                course.CreditHours = int.Parse(courseNode.Attributes["creditHours"].Value);
                //course.Title = courseNode.Attributes["title"].Value;
                course.Prerequisites = new List<Course>();
                course.Corequisites = new List<Course>();
                foreach (XmlNode prereqNode in courseNode.SelectNodes("prereqs/option"))
                {
                    string prereqID = prereqNode.Attributes["courseID"].Value;
                    if (!courseDict.ContainsKey(prereqID))
                    {
                        courseDict[prereqID] = new Course() { CourseID = prereqID };
                    }
                    course.Prerequisites.Add(courseDict[prereqID]);
                }
                foreach (XmlNode coreqNode in courseNode.SelectNodes("coreqs/option"))
                {
                    string coreqID = coreqNode.Attributes["courseID"].Value;
                    if (!courseDict.ContainsKey(coreqID))
                    {
                        courseDict[coreqID] = new Course() { CourseID = coreqID };
                    }
                    course.Corequisites.Add(courseDict[coreqID]);
                }
                courses.Add(course);
                courseDict[course.CourseID] = course;
            }
        }
        //extract major data
        foreach (XmlNode majorNode in majorNodes)
        {
            if (majorNode.Attributes["majorTitle"] != null && majorNode.Attributes["field"] != null)
            {
                Major major = new Major();
                major.MajorID = majorNode.Attributes["majorTitle"].Value;
                major.Field = majorNode.Attributes["field"].Value;
                //course.Title = courseNode.Attributes["title"].Value;
                major.Requisites = new List<Course>();
                foreach (XmlNode reqNode in majorNode.SelectNodes("course"))
                {
                    string reqID = reqNode.Attributes["courseID"].Value;
                    if (!courseDict.ContainsKey(reqID))
                    {
                        continue;
                    }
                    major.Requisites.Add(courseDict[reqID]);
                }
                majors.Add(major);
                majorDict[major.MajorID] = major;
            }
        }
        //extract minor data
        foreach (XmlNode minorNode in minorNodes)
        {
            if (minorNode.Attributes["minorTitle"] != null && minorNode.Attributes["field"] != null)
            {
                Minor minor = new Minor();
                minor.MinorID = minorNode.Attributes["minorTitle"].Value;
                minor.Field = minorNode.Attributes["field"].Value;
                //course.Title = courseNode.Attributes["title"].Value;
                minor.Requisites = new List<Course>();
                foreach (XmlNode reqNode in minorNode.SelectNodes("course"))
                {
                    string reqID = reqNode.Attributes["courseID"].Value;
                    if (!courseDict.ContainsKey(reqID))
                    {
                        continue;
                    }
                    minor.Requisites.Add(courseDict[reqID]);
                }
                minors.Add(minor);
                minorDict[minor.MinorID] = minor;
            }
        }
        // Build the directed graph
        graph = new Dictionary<Course, List<Course>>();
        foreach (Course course in courses)
        {
            graph[course] = new List<Course>();
            foreach (Course prereq in course.Prerequisites)
            {
                graph[prereq].Add(course);
            }
            foreach (Course coreq in course.Corequisites)
            {
                graph[course].Add(coreq);
                graph[coreq].Add(course);
            }
        }
        // Perform topological sort
        List<Course> sortedCourses = new List<Course>();
        Dictionary<Course, int> incomingEdges = new Dictionary<Course, int>();
        foreach (Course course in courses)
        {
            incomingEdges[course] = 0;
        }
        foreach (Course course in courses)
        {
            foreach (Course neighbor in graph[course])
            {
                incomingEdges[neighbor]++;
            }
        }
        Queue<Course> queue = new Queue<Course>(courses.Where(course => incomingEdges[course] == 0));
        while (queue.Any())
        {
            Course course = queue.Dequeue();
            sortedCourses.Add(course);
            foreach (Course neighbor in graph[course])
            {
                incomingEdges[neighbor]--;
                if (incomingEdges[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        //sort major req courses
        foreach(Major major in majors)
        {
            major.Requisites = major.Requisites.OrderBy(c => courses.IndexOf(c)).ToList();
        }
        //sort minor req courses
        foreach (Minor minor in minors)
        {
            minor.Requisites = minor.Requisites.OrderBy(c => courses.IndexOf(c)).ToList();
        }

        // Print the sorted courses
        //Debug.Log("Topologically sorted courses:");
        //foreach (Course course in majorDict["Journalism, Media Studies Concentration, B.S."].Requisites)
        //{
        //    Debug.Log(course.CourseID);
        //}
        UpdateUI();
    }



}