﻿using dj_webdesigncore.DTOs;
using dj_webdesigncore.DTOs.Admin;
using dj_webdesigncore.DTOs.Lobby;
using dj_webdesigncore.DTOs.Post;
using dj_webdesigncore.Entities.BusinessEntity;
using dj_webdesigncore.Entities.CourseEntity;
using dj_webdesigncore.Entities.UserEntity;
using dj_webdesigncore.Enums.ApiEnums;
using dj_webdesigncore.Request.Blog;
using dj_webdesigncore.Request.Chapter;
using dj_webdesigncore.Request.Course;
using dj_webdesigncore.Request.Lesson;
using dj_webdesigncore.Request.Post;
using dj_webdesigncore.Request.SomeThingElse;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dj_webdesigncore.Business.Admin
{
    public interface IAdminBusiness
    {
        Task<ResponData<GetLessonDTO>> getLessonPage(int page);
        Task<ResponData<GetChapterDTO>> getChapterPage(int page);
        Task<ResponData<GetCourseDTO>> getCoursePage(int page);
        Task<ResponData<AddLessonDTO>> addPracticeLesson(Request.Lesson.PracticeLesson practiceLesson);
        Task<ResponData<AddLessonDTO>> updatePracticeLesson(int lessonId, Request.Lesson.PracticeLesson practiceLesson);
        Task<ResponData<AddLessonDTO>> addTheoryLesson(TheoryLessonRequest theoryLessonRequest);
        Task<ResponData<AddLessonDTO>> updateTheoryLesson(int lessonId, TheoryLessonRequest theoryLessonRequest);
        Task<ResponData<AddLessonDTO>> addQuestionLesson(QuestionLessonRequest questionLessonRequest);
        Task<ResponData<AddLessonDTO>> updateQuestionLesson(int lessonId, QuestionLessonRequest questionLessonRequest);
        Task<ResponData<List<SelectCourseForChapter>>> selectCourseForChapter();
        Task<ResponData<AddChapterDTO>> addChapter(AddChapterRequest addChapterRequest);
        Task<ResponData<AddChapterDTO>> updateChapter(UpdateChapterRequest updateChapterRequest);
        Task<ResponData<List<TestCaseDTO>>> getAllTestCase(int practiceId);
        Task<ResponData<AddTestCaseDTO>> addTestCase(TestCaseRequest testCaseRequest);
        Task<ResponData<AddTestCaseDTO>> updateTestCase(int testCaseId, TestCaseRequest testCaseRequest);
        Task<ResponData<AddTestCaseDTO>> deleteTestCase(int testCaseId);
        Task<ResponData<List<DTOs.Admin.LessonDetailDTO>>> getLessonOfChapter(int chapterId);
        Task<ResponData<AddLesson2ChapterDTO>> updateSortNumberLesson(UpdateSortNumberLessonRequest updateSortNumberLessonRequest);
        Task<ResponData<AddLesson2ChapterDTO>> addLesson2Chapter(UpdateSortNumberLessonRequest updateSortNumberLessonRequest);
        Task<ResponData<AddLesson2ChapterDTO>> deleteLessonOfChapter(int lessonChapterId);
        Task<ResponData<List<DTOs.Admin.LessonDetailDTO>>> getLessonNotInChapter();
        Task<ResponData<OptionAddCourse>> getOptionAddCourse();
        Task<ResponData<ActionStatus>> addCourse(IFormFile img, AddCourseRequest addCourseRequest);
        Task<ResponData<ActionStatus>> updateCourse(IFormFile? img, AddCourseRequest addCourseRequest);
        Task<ResponData<List<ChapterCourseInfo>>> getChapterOfCourse(int courseId);
        Task<ResponData<ActionStatus>> updateCourseChapterSortNumber(int courseChapterId, int newSortNumber);
        Task<ResponData<ActionStatus>> deleteCourseChapter(int courseChapterId);
        Task<ResponData<List<MultiLangueDTO>>> getMultiLangueSuport(int practiceLessonId);
        Task<ResponData<List<Langue>>> getLangueHaveNot(int practiceLessonId);
        Task<ResponData<ActionStatus>> addMultiLangue(AddMultiLangue addMultiLangue);
        Task<ResponData<ActionStatus>> updateMultiLangue(AddMultiLangue addMultiLangue);
        Task<ResponData<GetLessonDTO>> findLesson(string key);
        Task<ResponData<ActionStatus>> addBlog(IFormFile img, AddBlogRequest addBlogRequest);
        Task<ResponData<List<BlogDTO>>> getBlog(int page);
        Task<ResponData<List<BlogDTO>>> findBlog(string key);
        Task<ResponData<ActionStatus>> updateBlog(IFormFile img, AddBlogRequest addBlogRequest);
        Task<ResponData<List<DenounceReportADMIN>>> getDenouncePage(int page);
        Task<ResponData<ActionStatus>> checkDenounce(ConfirmDenounceRequest confirmDenounce);
        Task<ResponData<HomeContent>> getHomeContent();
        Task<ResponData<ActionStatus>> changeSlide(IFormFile? slide1, IFormFile? slide2, IFormFile? slide3, IFormFile? slide4, IFormFile? slide5);
        Task<ResponData<ListPostAdmin>> getPostAdmin(int? statusOptId, int page);
        Task<ResponData<ActionStatus>> updatePost(IFormFile? img, UpdatePostAdminRequest updatePostAdmin);
        Task<IQueryable<User>> GetUserLockPage(int page);
        Task<ResponData<AnalyticAppDTO>> GetAnalytics();
        Task<ResponData<UserAnalyticCustomDTO>> GetAnalyticsFilter(DateTime start, DateTime end);
        Task<List<AdviceContactDTO>> GetAdviceContact(int page);
        Task<AdviceContact> getAdviceDetail(int adviceId);
        Task<CourseAnaDTO> getCourseAna(int type = -1);
        Task<List<ListStudentOfCourse>> ListStudentOfCourse(int courseId, int page);
        Task<UserCourseProgress> GetCourseProgressUser(int courseId, int userId);
        Task<ActionStatus> lockOrUnlockLesson(int userId, int lessonId);
        Task<VideoDoneData> GetVideoDoneDataOfUser(int userId, int videoLessonId);
        Task<PracticeDoneData> GetPracDoneDataOfUser(int userId, int pracId);
        Task<QuestionDoneData> GetQuesDoneDataOfUser(int userId, int quesId);
        Task<UserShowDTO> GetUserShow(int userId);
        Task<List<AdviceByItemDTO>> GetAdviceAna(int opt = 1);
        Task<List<ListStudentOfCourse>> GetCourseAnaType(int courseId, int page, int opt = 1);
    }
}