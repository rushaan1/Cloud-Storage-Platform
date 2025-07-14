import {Component, OnDestroy, OnInit, AfterViewInit, ElementRef, ViewChild} from '@angular/core';
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import { phoneNumberValidator } from '../../Utils';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('analyticsContainer', { static: false }) analyticsContainer!: ElementRef;
  chartView: [number, number] = [400, 400];
  barChartView: [number, number] = [500, 300];
  private resizeObserver!: ResizeObserver;

  updateAccountMode = false;
  updateForm!: FormGroup;
  countries: string[] = [
    'United States', 'Canada', 'United Kingdom', 'Australia', 'Germany',
    'France', 'Spain', 'Italy', 'Japan', 'China', 'India', 'Brazil',
    'Mexico', 'Russia', 'South Africa'
  ];
  deleteConfirmMode = false;

  constructor(private filesState:FilesStateService, private fb: FormBuilder) {}
  pieData = [
    { name: 'Folder A', value: 1200 },
    { name: 'Folder B', value: 800 },
    { name: 'Folder C', value: 500 }
  ];
  barData = [
    { name: 'Folder ABCDE', value: 1000 },
    { name: 'Folder B', value: 750 },
    { name: 'Folder C', value: 600 },
    { name: 'Folder D', value: 200 },
    { name: 'Folder E', value: 400 },
    { name: 'Folder F', value: 350 },
    { name: 'Folder G', value: 100 },
    { name: 'Folder H', value: 800 },
    { name: 'Folder I', value: 660 },
    { name: 'Folder J', value: 900 },
  ];


  ngOnInit(): void {
    this.filesState.outsideFilesAndFoldersMode = true;
    this.filesState.showSpaceUtilizedInNavDrawer = false;
    this.updateForm = this.fb.group({
      email: ['sample(a)gmail.com', [Validators.required, Validators.email]],
      fullName: ['Full user name', [Validators.required]],
      country: ['India', [Validators.required]],
      phoneNumber: ['+1234567890', [phoneNumberValidator]],
      password: ['', [Validators.minLength(6)]],
      confirmPassword: ['']
    }, { validators: this.passwordMatchValidator });
  }

  onUpdateDetailsClick() {
    this.updateAccountMode = true;
  }

  onCancelUpdate() {
    this.updateAccountMode = false;
    this.updateForm.reset({
      email: 'sample(a)gmail.com',
      fullName: 'Full user name',
      country: 'India',
      phoneNumber: '+1234567890',
      password: '',
      confirmPassword: ''
    });
  }

  onConfirmUpdate() {
    if (this.updateForm.valid) {
      // Submit update logic here
      this.updateAccountMode = false;
      this.onCancelUpdate();
    } else {
      this.updateForm.markAllAsTouched();
    }
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      return null;
    }
  }

  onDeleteAccountClick() {
    this.deleteConfirmMode = true;
  }
  onCancelDelete() {
    this.deleteConfirmMode = false;
  }
  onConfirmDelete() {
    // TODO: Add actual delete logic here
    this.deleteConfirmMode = false;
  }

  ngAfterViewInit(): void {
    this.setChartSizes();
    this.resizeObserver = new ResizeObserver(() => {
      this.setChartSizes();
    });
    if (this.analyticsContainer && this.analyticsContainer.nativeElement) {
      this.resizeObserver.observe(this.analyticsContainer.nativeElement);
    }
    window.addEventListener('resize', this.setChartSizes.bind(this));
  }

  setChartSizes() {
    let width = window.innerWidth;
    let pieSize = 400;
    let barWidth = 500;
    let barHeight = 300;
    if (width < 1186 && width > 854) {
      pieSize = 260;
      barWidth = 320;
      barHeight = 180;
    }
    if (width < 856) {
      pieSize = 240;
      barWidth = 250;
      barHeight = 180;
    }
    this.chartView = [pieSize, pieSize];
    this.barChartView = [barWidth, barHeight];
  }

  ngOnDestroy(): void {
    this.filesState.outsideFilesAndFoldersMode = false;
    this.filesState.showSpaceUtilizedInNavDrawer = true;
    if (this.resizeObserver && this.analyticsContainer && this.analyticsContainer.nativeElement) {
      this.resizeObserver.unobserve(this.analyticsContainer.nativeElement);
    }
    window.removeEventListener('resize', this.setChartSizes.bind(this));
  }
}
